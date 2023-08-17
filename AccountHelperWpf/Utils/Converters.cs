namespace AccountHelperWpf.Utils;

static class Converters
{
    public static string ToGoodString(this decimal sum) => sum.ToString(sum % 1 == 0 ? "G0" : "#.##");
}
