using System.Collections;

namespace AccountHelperWpf.Utils;

public static class CollectionHelper
{
    public static IEnumerable CheckNull(this IEnumerable? collection) => collection ?? Enumerable.Empty<object>();
}