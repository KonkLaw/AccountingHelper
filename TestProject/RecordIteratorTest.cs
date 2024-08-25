using AccountHelperWpf.Parsing;

namespace TestProject;

[TestClass]
public class RecordIteratorTest
{
    [TestMethod]
    public void TestSimple_old()
    {
        string test =
            @"""2000-02-11"",""2000-02-11"",""Płatność kartą"",""-123.00"",""PLN"",""+345.86"",""Tytuł : 0003"",""Lokalizacja : Adres : I.- Miasto : Asddsf Kraj : Qwerr"",""Data i czas operacji : 2000-02-11"",""Oryginalna kwota operacji : 123.00"",""""";
        const string firstPart = "2000-02-11";
        const string lastPart = "";
        RunTest(test, firstPart, lastPart, 11);
    }

    [TestMethod]
    public void TestComplex1_old()
    {
        string test =
            @"""2000-02-11"",""2024-02-02"",""Płatność kartą"",""-123.00"",""PLN"",""+867.75"",""Tytuł : 3453"",""Lokalizacja : Adres : RESTAURACJA  JEFF""S Miasto : Www Kraj : Asd"",""Data i czas operacji : 2000-02-11"",""Oryginalna kwota operacji : 345.00"",""""";
        const string firstPart = "2000-02-11";
        const string lastPart = "";
        RunTest(test, firstPart, lastPart, 11);
    }

    [TestMethod]
    public void TestComplex2_old()
    {
        string test =
            @"""2000-02-11"",""2000-02-11"",""Płatność kartą"",""-123.00"",""PLN"",""+546.75"",""Tytuł : 3453"",""Lokalizacja : Adres : RESTAURACJA  JEFF""S Miasto : Www Kraj : Asd"",""Data i czas operacji : 2000-02-11"","""",""""";
        const string firstPart = "2000-02-11";
        const string lastPart = "";
        RunTest(test, firstPart, lastPart, 11);
    }

    [TestMethod]
    public void TestComplex1()
    {
	    string test =
			@"""2020-08-20"",""2020-08-20"",""Płatność kartą"",""-123.00"",""PLN"",""+546.75"",""Tytu³:  3453 "",""Lokalizacja: Adres: Qweasfsdf Miasto: Www Kraj: Asd"",""Data i czas operacji: 2020-08-20"",""Oryginalna kwota operacji: 99.99"",""Numer karty: 123******123"","""",""""";
	    const string firstPart = "2020-08-20";
	    const string lastPart = "";
	    RunTest(test, firstPart, lastPart, 13);
    }

	private void RunTest(string text, string firstPart, string lastPart, int iterationsCount)
    {
        RecordIterator iterator = new RecordIterator(text);
        int count = 0;
        while (iterator.TryGetNextSpan(out ReadOnlySpan<char> span))
        {
            if (count == 0)
            {
                Assert.IsTrue(span.SequenceEqual(firstPart));
            }
            else if (count == iterationsCount - 1)
            {
                Assert.IsTrue(span.SequenceEqual(lastPart));
            }
            count++;
        }
        Assert.AreEqual(count, iterationsCount);
    }
}