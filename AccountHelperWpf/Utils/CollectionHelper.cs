using System.Collections;

namespace AccountHelperWpf.Utils;

public static class CollectionHelper
{
    public static void PrependBeforeBigger<T>(this IList<T> collection, T valueToInsert, Func<T, T, int> comparator)
    {
        int i = 0;
        for (; i < collection.Count; i++)
        {
            if (comparator(collection[i], valueToInsert) >= 0)
            {
                collection.Insert(i, valueToInsert);
                return;
            }
        }
        collection.Add(valueToInsert);
    }

    public static KeyValuePair<TKey, TValue>[] ToArray<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        // in .Net sorted dictionary to array is not optimized for know count
        var result = new KeyValuePair<TKey, TValue>[dictionary.Count];
        int i = 0;
        foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
        {
            result[i++] = keyValuePair;
        }
        return result;
    }

    public static IEnumerable CheckNull(this IEnumerable? collection)
    {
        if (collection == null)
            return Enumerable.Empty<object>();
        return collection;
    }
}