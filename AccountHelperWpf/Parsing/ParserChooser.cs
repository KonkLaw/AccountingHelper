using System.IO;
using System.Text;
using AccountHelperWpf.Common;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Parsing;

class ParserChooser
{
    public static AccountFile? ParseFile(string filePath, IViewResolver viewResolver)
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

            OperationsGroup? operationsGroup = PkoParser.TryParse(reader);
            if (operationsGroup != null)
            {
                PkoBlockedOperationParserVM pkoBlockedOperationsVM = new (viewResolver);
                viewResolver.ResolveAndShowDialog(pkoBlockedOperationsVM);

                var operationsGroups = new List<OperationsGroup>
                {
                    operationsGroup.Value
                };
                if (pkoBlockedOperationsVM.OperationsGroup.HasValue)
                    operationsGroups.Insert(0, pkoBlockedOperationsVM.OperationsGroup.Value);

                return new AccountFile(new AccountDescription(fileName, "zl"), operationsGroups);
            }
        }

        return null;
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