using System;
using System.Globalization;
using System.Threading.Tasks;

namespace AccountHelperWpf.Models;

public class StringDistance
{
    public static float? GetDistancePercent(string s1, string s2)
    {
        if (s1 == s2)
            return 0f;

        const float tolerance = 0.14f;
        float averageSize = (s1.Length + s2.Length) / 2f;
        float distanceLimit = tolerance * averageSize;
        int distanceLimitInt = (int)Math.Ceiling(distanceLimit);
        int distance = GetDistance(s1, s2, distanceLimitInt);

        if (distance >= distanceLimitInt)
            return null;

        return distance / averageSize;
    }

    private static unsafe int GetDistance(string s1In, string s2In, int stopSearchDistance)
    {
        if (s1In.Length > s2In.Length)
            (s1In, s2In) = (s2In, s1In);

        Span<char> s1 = stackalloc char[s1In.Length];
        Span<char> s2 = stackalloc char[s2In.Length];
        MemoryExtensions.ToLower(s1In, s1, CultureInfo.InvariantCulture);
        MemoryExtensions.ToLower(s2In, s2, CultureInfo.InvariantCulture);

        Span<int> d0 = stackalloc int[s1.Length + 1];
        Span<int> d1 = stackalloc int[d0.Length];

        for (int i = 0; i < d0.Length; i++)
            d0[i] = i;

        for (int i2 = 0; i2 < s2.Length; i2++)
        {
            d1[0] = i2 + 1;
            int fastExitCurrentMin = int.MaxValue;

            for (int i1 = 0; i1 < s1.Length; i1++)
            {
                bool isEquals = s1[i1] == s2[i2];

                int newDistance = Math.Min(
                    Math.Min(d1[i1] + 1, d0[i1 + 1] + 1),
                    d0[i1] + (isEquals ? 0 : 1));
                d1[i1 + 1] = newDistance;

                if (newDistance < fastExitCurrentMin)
                {
                    fastExitCurrentMin = newDistance;
                }
            }

            if (fastExitCurrentMin >= stopSearchDistance)
                return stopSearchDistance;

            Span<int> temp = d0;
            d0 = d1;
            d1 = temp;
        }
        return d0[^1];
    }
}

public class CollectionSearchHelper
{
    private static TResult Run<TItem, TContainer, TResult>(
        string target, IReadOnlyList<TItem> collection,
        Func<TItem, string> selector, TContainer[] containers)
        
        where TItem : class
        where TContainer : IProcessor<TItem, TContainer, TResult>
    {
        int processorCount = Environment.ProcessorCount;
        if (collection.Count < 20 || collection.Count < processorCount)
        {
            ProcessRange<TItem, TContainer, TResult>(target, collection, selector, ref containers[0], 0, collection.Count);
        }
        else
        {
            int batchSize = collection.Count / processorCount + (collection.Count % processorCount == 0 ? 0 : 1);
            Parallel.For(0, processorCount, index =>
            {
                int startIndex = index * batchSize;
                int stopIndex = Math.Min(startIndex + batchSize, collection.Count);
                ProcessRange<TItem, TContainer, TResult>(target, collection, selector, ref containers[index],
                    startIndex, stopIndex);
            });
        }

        return TContainer.PrecessSelected(containers);
    }

    private static void ProcessRange<TItem, TContainer, TResult>(
        string target,
        IReadOnlyList<TItem> collection, Func<TItem, string> selector,
        ref TContainer container,
        int startIndexInc, int stopIndexExc)       
        where TItem : class
        where TContainer : IProcessor<TItem, TContainer, TResult>
    {
        for (int i = startIndexInc; i < stopIndexExc; i++)
        {
            TItem item = collection[i];
            float? dist = StringDistance.GetDistancePercent(target, selector(item));
            if (dist.HasValue)
            {
                if (TContainer.ProcessItemsAndGetStop(ref container, dist.Value, item))
                    return;
            }
        }
    }


    public static IReadOnlyList<T> FindAll<T>(string target, IReadOnlyList<T> collection, Func<T, string> selector) where T : class
    {
        int processorCount = Environment.ProcessorCount;
        SelectAllProcessor<T> processor = new SelectAllProcessor<T>();
        SelectAllProcessor<T>[] containers = Enumerable.Range(0, processorCount).Select(_ => processor).ToArray();

        return Run<T, SelectAllProcessor<T>, IReadOnlyList<T>>(target, collection, selector, containers);
    }

    public static T? FindBest<T>(string target, IReadOnlyList<T> collection, Func<T, string> selector) where T : class
    {
        int processorCount = Environment.ProcessorCount;
        FindBestProcessor<T>[] results = new FindBestProcessor<T>[processorCount];
        return Run<T, FindBestProcessor<T>, T?>(target, collection, selector, results);
    }

    public static bool FindAny<T>(string target, IReadOnlyList<T> collection, Func<T, string> selector) where T : class
    {
        int processorCount = Environment.ProcessorCount;
        FindAnyProcessor<T>[] results = new FindAnyProcessor<T>[processorCount];
        return Run<T, FindAnyProcessor<T>, bool>(target, collection, selector, results);
    }
}



// ------------------------------



interface IProcessor<in TItem, TContainer, out TResult> where TItem : class
{
    static abstract bool ProcessItemsAndGetStop(ref TContainer container, float newDistance, TItem item);
    static abstract TResult PrecessSelected(TContainer[] containers);
}

struct FindBestProcessor<T> : IProcessor<T, FindBestProcessor<T>, T?> where T : class
{
    private T? minObj;
    private float? minDistance;

    public static bool ProcessItemsAndGetStop(ref FindBestProcessor<T> container, float newDistance, T item)
    {
        if (!container.minDistance.HasValue || newDistance < container.minDistance.Value)
        {
            container.minDistance = newDistance;
            container.minObj = item;
            if (newDistance == 0)
                return true;
        }
        return false;
    }

    public static T? PrecessSelected(FindBestProcessor<T>[] containers)
    {
        T? minObj = null;
        float? minDistance = null;
        for (int i = 0; i < containers.Length; i++)
        {
            float? dist = containers[i].minDistance;
            T? obj = containers[i].minObj;
            if (dist.HasValue && (!minDistance.HasValue || minDistance.Value > dist.Value))
            {
                minDistance = dist;
                minObj = obj;
            }
        }
        return minObj;
    }
}

struct FindAnyProcessor<T> : IProcessor<T, FindAnyProcessor<T>, bool> where T : class
{
    private bool wasAny;

    public static bool ProcessItemsAndGetStop(ref FindAnyProcessor<T> container, float newDistance, T item)
    {
        container.wasAny = true;
        return true;
    }

    public static bool PrecessSelected(FindAnyProcessor<T>[] results)
    {
        for (int i = 0; i < results.Length; i++)
        {
            FindAnyProcessor<T> findAnyProcessor = results[i];
            if (findAnyProcessor.wasAny)
                return true;
        }
        return false;
    }
}

readonly struct SelectAllProcessor<T> : IProcessor<T, SelectAllProcessor<T>, IReadOnlyList<T>> where T : class
{
    private static readonly object SynchLock = new();

    private readonly List<T> list;

    public SelectAllProcessor()
    {
        list = new List<T>();
    }

    public static bool ProcessItemsAndGetStop(ref SelectAllProcessor<T> container, float newDistance, T item)
    {
        lock (SynchLock)
        {
            container.list.Add(item);
        }
        return false;
    }

    public static IReadOnlyList<T> PrecessSelected(SelectAllProcessor<T>[] results)
    {
        return results[0].list;
    }
}