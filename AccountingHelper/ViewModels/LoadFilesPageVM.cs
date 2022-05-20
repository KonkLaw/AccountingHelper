using AccountingHelper.Logic;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;

namespace AccountingHelper.ViewModels;

class LoadFilesPageVM : ILoadFilesPageVM
{
    private readonly Storage storage;

    public List<AccountFile> LoadedFiles { get; }

    public string HistoryFileInfo
        => storage.History == null ? "(Empty)" : storage.History.Name;

    public LoadFilesPageVM(Storage storage)
    {
        LoadedFiles = new List<AccountFile>();
        this.storage = storage;
        storage.list = LoadedFiles;
    }

    public async Task AddFile(InputFileChangeEventArgs e)
    {
        AccountFile file = await ParseFile(e);
        LoadedFiles.Add(file);
    }

    private static async Task<AccountFile> ParseFile(InputFileChangeEventArgs e)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var lines = new List<string>();
        using (Stream fs = e.File.OpenReadStream())
        using (StreamReader reader = new(fs, Encoding.GetEncoding("windows-1251")))
        {
            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (line == null)
                    break;
                else
                    lines.Add(line);
            }
        }

        List<RecordGroup> records = ParsingHelper.ParseLines(lines, out string currency);

        return new AccountFile(new AccountDescription(e.File.Name, currency), records);
    }

    public void CreateHistoryFile()
    {
        storage.History = History.CreateNewHistory();
    }
}

internal interface ILoadFilesPageVM
{
    string HistoryFileInfo { get; }
    List<AccountFile> LoadedFiles { get; }
    Task AddFile(InputFileChangeEventArgs e);
    void CreateHistoryFile();
}