using AccountingHelper.Logic;
using AccountingHelper.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;

namespace AccountingHelper.ViewModels;

class LoadFilesPageVM : ILoadFilesPageVM
{
    private readonly IServiceProvider serviceProvider;

    public List<AccountFile> LoadedFiles { get; }

    public LoadFilesPageVM(IServiceProvider provider)
    {
        LoadedFiles = new List<AccountFile>();
        this.serviceProvider = provider;
    }

    public async Task LoadFile(InputFileChangeEventArgs e)
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

    public void ClearFiles()
    {
        LoadedFiles.Clear();
    }

    public void Next()
    {
        Storage storage = serviceProvider.GetService<Storage>()!;
        storage.list = LoadedFiles;
        serviceProvider.GetService<NavigationManager>()!.NavigateTo(nameof(SelectionPage));
    }
}

internal interface ILoadFilesPageVM
{
    List<AccountFile> LoadedFiles { get; }
    Task LoadFile(InputFileChangeEventArgs e);
    void ClearFiles();
    void Next();
}