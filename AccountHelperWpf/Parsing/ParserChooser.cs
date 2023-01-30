using System.IO;
using System.IO.Pipes;
using System.Text;

namespace AccountHelperWpf.Parsing;

public class ParserChooser
{
    public static AccountFile ParseFile(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            StreamReader reader = new (fileStream, EncodingHelper.RusEncoding);
            AccountFile? result = PriorParser.TryParse(reader, fileName);
            if (result != null)
                return result;

            fileStream.Position = 0;

            reader = new (fileStream, EncodingHelper.PolandEncoding);
            result = PkoParser.TryParse(reader, fileName);
            if (result != null)
                return result;
        }

        throw new Exception("File wasn't recognized by any parser.");
    }
}

file static class EncodingHelper
{
    public static readonly Encoding RusEncoding;
    public static readonly Encoding PolandEncoding;

    static EncodingHelper()
    {
        // Required for loading of Russian encoding.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        RusEncoding = Encoding.GetEncoding("windows-1251");
        PolandEncoding = Encoding.GetEncoding("windows-1250");
    }
}