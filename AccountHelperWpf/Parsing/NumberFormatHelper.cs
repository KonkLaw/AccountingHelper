using System.Globalization;

namespace AccountHelperWpf.Parsing;

class NumberFormatHelper
{
    public static readonly NumberFormatInfo NumberFormat;

    static NumberFormatHelper()
    {
        NumberFormat = (NumberFormatInfo)CultureInfo.InstalledUICulture.NumberFormat.Clone();
        NumberFormat.NumberDecimalSeparator = ",";
        NumberFormat.NumberGroupSeparator = " ";
    }
}