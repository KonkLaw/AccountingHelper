namespace AccountHelperWpf.Parsing.Pko;

/// <summary>
/// PKO exports list operations newest-first, but the order does not follow the running balance:
/// <see cref="PkoOperation.SaldoAfterTransaction"/> tracks settlement (posting) order while the
/// date columns hold the transaction date, and card payments settle a few days after the swipe.
/// As a result the rows are shuffled relative to the true balance progression, and the shuffling
/// crosses day boundaries, so it cannot be fixed per-day.
///
/// Every row carries both its amount and its resulting balance, so adjacency can be recovered:
/// operation A immediately precedes B iff <c>saldoAfter(A) == saldoAfter(B) - amount(B)</c> and the
/// two are close in time. Because the file is only a time window, the operations do not form a
/// single chain - operations near the newest end may have their successor outside the file, and
/// operations near the oldest end may have their predecessor outside it. The reconstruction
/// therefore produces one or more chained fragments, ordered by their position in the source file
/// so the bank's newest-first layout is preserved while each fragment is internally corrected.
/// </summary>
static class PkoSaldoSorter
{
    /// <summary>
    /// Two operations are never linked when their dates differ by more than this many days, even
    /// when the balances are the only possible match: across that span a balance match is more
    /// likely a coincidence (a balance recurring later) than a real adjacency.
    /// </summary>
    private const int DefaultMaxLinkDayDifference = 7;

    public static IReadOnlyList<PkoOperation> Sort(
        IReadOnlyList<PkoOperation> operations,
        int maxLinkDayDifference = DefaultMaxLinkDayDifference)
    {
        int n = operations.Count;
        if (n <= 1)
            return operations;

        decimal[] balanceAfter = new decimal[n];
        decimal[] balanceBefore = new decimal[n];
        for (int i = 0; i < n; i++)
        {
            // Without a balance on every row the chain cannot be built; keep the source order.
            if (operations[i].SaldoAfterTransaction is not { } saldoAfter)
                return operations;
            balanceAfter[i] = saldoAfter;
            balanceBefore[i] = saldoAfter - operations[i].Amount;
        }

        // Candidate links A -> B: B starts where A ended (balanceBefore[B] == balanceAfter[A]).
        // Group operations by the balance they start from so each A finds its candidates directly.
        Dictionary<decimal, List<int>> startingFrom = [];
        for (int i = 0; i < n; i++)
        {
            if (!startingFrom.TryGetValue(balanceBefore[i], out List<int>? list))
                startingFrom[balanceBefore[i]] = list = [];
            list.Add(i);
        }

        List<(int DayDifference, int From, int To)> links = [];
        for (int i = 0; i < n; i++)
        {
            if (!startingFrom.TryGetValue(balanceAfter[i], out List<int>? candidates))
                continue;
            foreach (int to in candidates)
            {
                if (to == i)
                    continue;
                int dayDifference = Math.Abs(
                    operations[i].DateAccounting.DayNumber - operations[to].DateAccounting.DayNumber);
                if (dayDifference <= maxLinkDayDifference)
                    links.Add((dayDifference, i, to));
            }
        }

        // Build a linear forest: prefer the closest-in-time links, and accept a link only when both
        // endpoints are still free and the two are not already in one chain (which would form a
        // cycle - e.g. a transfer out and straight back returning to the same balance).
        links.Sort(static (x, y) =>
        {
            int byDays = x.DayDifference.CompareTo(y.DayDifference);
            if (byDays != 0)
                return byDays;
            int byFrom = x.From.CompareTo(y.From);
            return byFrom != 0 ? byFrom : x.To.CompareTo(y.To);
        });

        int[] successor = new int[n];
        int[] predecessor = new int[n];
        Array.Fill(successor, -1);
        Array.Fill(predecessor, -1);
        DisjointSet chains = new(n);

        foreach ((_, int from, int to) in links)
        {
            if (successor[from] != -1 || predecessor[to] != -1 || chains.Find(from) == chains.Find(to))
                continue;
            successor[from] = to;
            predecessor[to] = from;
            chains.Union(from, to);
        }

        // Emit each chain newest-first (from its tail back to its head). Chains appear in the order
        // their newest row is first met while scanning the source file, keeping its overall layout.
        List<PkoOperation> result = new(n);
        bool[] emitted = new bool[n];
        for (int i = 0; i < n; i++)
        {
            if (emitted[i])
                continue;
            int tail = i;
            while (successor[tail] != -1)
                tail = successor[tail];
            for (int current = tail; current != -1; current = predecessor[current])
            {
                emitted[current] = true;
                // The previous (older) operation sits below this one in the newest-first output;
                // it is present only when this operation has a chained predecessor.
                result.Add(operations[current] with { IsLinkedToPrevious = predecessor[current] != -1 });
            }
        }
        return result;
    }

    private sealed class DisjointSet
    {
        private readonly int[] parent;

        public DisjointSet(int size)
        {
            parent = new int[size];
            for (int i = 0; i < size; i++)
                parent[i] = i;
        }

        public int Find(int x)
        {
            while (parent[x] != x)
                x = parent[x] = parent[parent[x]];
            return x;
        }

        public void Union(int a, int b) => parent[Find(a)] = Find(b);
    }
}
