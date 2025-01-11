namespace AccountHelperWpf.Parsing.Pko;

public struct RecordIterator
{
    private readonly string record;
    private int index;

    public RecordIterator(string record)
    {
        this.record = record;
    }

    public bool TryGetNextSpan(out ReadOnlySpan<char> span)
    {
        const string separatorSet = "\",\"";
        int endIndex = record.Length - 1;

        if (index >= endIndex)
        {
            span = default;
            return false;
        }
        int textStartIndex = index == 0 ? 1 : index + separatorSet.Length;

        int textStopIndex = record.IndexOf(separatorSet, textStartIndex, StringComparison.InvariantCulture);
        if (textStopIndex < 0)
            textStopIndex = endIndex;

        span = record.AsSpan(textStartIndex, textStopIndex - textStartIndex);
        index = textStopIndex;
        return true;
    }

    public ReadOnlySpan<char> GetNextSpan()
    {
        TryGetNextSpan(out ReadOnlySpan<char> span);
        return span;
    }
}