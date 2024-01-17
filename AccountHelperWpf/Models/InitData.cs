using System;
using System.Collections.ObjectModel;
using System.Printing;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

record InitData
{
    public ObservableCollection<CategoryVM> Categories;
    public ObservableDictionary Associations;
    public ObservableHashset ExcludedOperations;

    public InitData(ObservableCollection<CategoryVM> categories, ObservableCollection<AssociationVM> associations, ObservableCollection<string> excludedOperations)
    {
        Categories = categories;
        Associations = new ObservableDictionary(associations);
        ExcludedOperations = new ObservableHashset(excludedOperations);
    }
}

class ObservableDictionary
{
    private readonly ObservableCollection<AssociationVM> collection;
    public IEnumerable<AssociationVM> Collection;

    public ObservableDictionary(ObservableCollection<AssociationVM> collection)
    {
        this.collection = collection;;
        Collection = collection;
    }

    public AssociationVM? TryGetBestMatch(string description)
        => TryGetBestMatch(description, out _);

    private AssociationVM? TryGetBestMatch(string description, out int index)
    {
        float minDist = float.MaxValue;
        AssociationVM? bestMath = null;
        int indexOfBest = -1;

        for (int i = 0; i < collection.Count; i++)
        {
            AssociationVM associationVM = collection[i];
            float? dist = StringDistance.GetDistancePercent(description, associationVM.OperationDescription);
            if (dist.HasValue && dist.Value < minDist)
            {
                if (dist.Value == 0f)
                {
                    index = i;
                    return associationVM;
                }
                minDist = dist.Value;
                bestMath = associationVM;
                indexOfBest = i;
            }
        }
        index = indexOfBest;
        return bestMath;
    }

    public void DeleteBetsMatch(string description)
    {
        if (TryGetBestMatch(description, out int index) != null)
            collection.RemoveAt(index);
    }

    public void Add(AssociationVM associationVM) =>
        collection.PrependBeforeBigger(associationVM,
            (o1, o2) => string.Compare(o1.OperationDescription, o2.OperationDescription, StringComparison.InvariantCulture));

    public AssociationVM GetByIndexAt(int index) => collection[index];

    public void DeleteAt(int index) => collection.RemoveAt(index);
}

class ObservableHashset
{
    private readonly ObservableCollection<string> collection;
    public IEnumerable<string> Collection => collection;

    public ObservableHashset(ObservableCollection<string> collection)
    {
        this.collection = collection;
    }

    public bool ContainsSimilar(string description)
    {
        for (int i = 0; i < collection.Count; i++)
        {
            float? dist = StringDistance.GetDistancePercent(description, collection[i]);
            if (dist.HasValue)
                return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        collection.RemoveAt(index);
    }

    public void Add(string description)
    {
        collection.PrependBeforeBigger(description, string.Compare);    
    }
}

class StringDistance
{
    public static float? GetDistancePercent(string s1, string s2)
    {
        if (s1 == s2)
            return 0f;

        int distance = GetDistance(s1, s2);
        float distancePercent = distance / ((s1.Length + s2.Length) / 2f);

        const float tolerance = 0.14f;
        if (distancePercent > tolerance)
            return null;
        return distancePercent;
    }

    private static unsafe int GetDistance(string s1, string s2)
    {
        if (s1.Length > s2.Length)
            (s1, s2) = (s2, s1);

        Span<int> d0 = stackalloc int[s1.Length + 1];
        Span<int> d1 = stackalloc int[d0.Length];

        for (int i = 0; i < d0.Length; i++)
            d0[i] = i;

        for (int i2 = 0; i2 < s2.Length; i2++)
        {
            d1[0] = i2 + 1;
            for (int i1 = 0; i1 < s1.Length; i1++)
            {
                bool isEquals = char.ToLower(s1[i1]) == char.ToLower(s2[i2]);

                d1[i1 + 1] = Math.Min(
                    Math.Min(d1[i1] + 1, d0[i1 + 1] + 1),
                    d0[i1] + (isEquals ? 0 : 1));
            }

            Span<int> temp = d0;
            d0 = d1;
            d1 = temp;
        }
        return d0[^1];
    }
}