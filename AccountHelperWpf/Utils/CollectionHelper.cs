using System.Collections;
using AccountHelperWpf.Models;

namespace AccountHelperWpf.Utils;

public static class CollectionHelper
{
    public static KeyValue[] ToArray(this SortedDictionary<string, string> dictionary)
    {
        // in .Net sorted dictionary to array is not optimized for know count
        var result = new KeyValue[dictionary.Count];
        int i = 0;
        foreach (KeyValuePair<string, string> keyValuePair in dictionary)
        {
            result[i++] = new KeyValue(keyValuePair.Key, keyValuePair.Value);
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