using System.Diagnostics;
using System.Text;
using AccountHelperWpf.Models;

namespace TestProject;

[TestClass]
public class StringDistanceTest
{
    private static readonly Random random = new Random();

    private static string GetRandomString(int size)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789/*-+!.,?";
        char[] stringChars = new char[size];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }


    [TestMethod]
    public void TestEquals()
    {
        string s1 = GetRandomString(8);
        float? res1 = StringDistance.GetDistancePercent(s1, s1);
        Assert.AreEqual(res1, 0);

        string s2 = GetRandomString(15);
        float? res2 = StringDistance.GetDistancePercent(s1, s1);
        Assert.AreEqual(res2, 0);

        string s3 = GetRandomString(28);
        float? res3 = StringDistance.GetDistancePercent(s1, s1);
        Assert.AreEqual(res3, 0);
    }


    [TestMethod]
    public void TestOrder()
    {
        float GetDisatance(string s1, string s2, int diffSize)
            => diffSize / ((s1.Length + s2.Length) / 2.0f);

        string s = GetRandomString(15);
        StringBuilder sb = new StringBuilder(s);

        float? d0 = StringDistance.GetDistancePercent(s, s);
        Assert.AreEqual(d0, 0);
        
        sb[4] = '$';
        string s1 = sb.ToString();
        float? d1 = StringDistance.GetDistancePercent(s, s1);
        Assert.AreEqual(d1, GetDisatance(s, s1, 1));

        sb.Remove(4, 1);
        string s2 = sb.ToString();
        float? d2 = StringDistance.GetDistancePercent(s, s2);
        Assert.AreEqual(d2, GetDisatance(s, s2, 1));

        sb.Append('$');
        string s3 = sb.ToString();
        float? d3 = StringDistance.GetDistancePercent(s, s3);
        Assert.AreEqual(d3, GetDisatance(s, s3, 2));
    }

    [TestMethod]
    public void TestParallel_Big()
    {
        RunCollectionTest(1000);
    }

    [TestMethod]
    public void TestParallel_Small()
    {
        RunCollectionTest(2);
    }

    private static void RunCollectionTest(int size)
    {
        string target = GetRandomString(17);
        List<string> data = new();

        StringBuilder sb = new StringBuilder(target)
        {
            [4] = '$'
        };
        target = sb.ToString();

        int testDataSize = size;
        for (int i = 0; i < testDataSize; i++)
        {
            data.Add(GetRandomString(17));
        }

        data[random.Next(data.Count)] = target;

        string? res = CollectionSearchHelper.FindBest(target, data, s => s);
        Assert.AreEqual(target, res);
    }
}