using System.IO;
using System.Text;
using AccountHelperWpf.Models;
using AccountHelperWpf.Parsing.Pko;
using AccountHelperWpf.ViewModels;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.Parsing;

class ParserChooser
{
    public static OperationsFile? ParseFile(string filePath, IViewResolver viewResolver)
    {
        string fileName = Path.GetFileName(filePath);
        try
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                StreamReader reader = new(fileStream, EncodingHelper.RusEncoding);
                PriorFile? result = PriorParser.TryParse(reader, fileName);
                if (result != null)
                    return Converter.Convert(result);

                fileStream.Position = 0;

                reader = new(fileStream, EncodingHelper.PolandEncoding);

                IReadOnlyList<PkoOperation>? nonBlockedOperations = PkoParser.TryParseFile(reader, out bool withSaldo);
                if (nonBlockedOperations != null)
                {
                    PkoBlockedOperationParserVM pkoBlockedOperationsVM = new(viewResolver);
                    // temp solution
                    //viewResolver.ResolveAndShowDialog(pkoBlockedOperationsVM);
                    return Converter.Convert(
                        new PkoFile(fileName, nonBlockedOperations, pkoBlockedOperationsVM.BlockedOperations, withSaldo));
                }

                viewResolver.ShowWarning("Sorry, the fle wasn't recognized as any known bank report.");
                return null;
            }
        }
        catch (IOException)
        {
            viewResolver.ShowWarning("File is already opened by other application or don't have access to file.");
            return null;
        }
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