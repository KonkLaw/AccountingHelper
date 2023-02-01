using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AccountHelperWpf.BaseObjects;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewModels;

class OpenFilesViewModel : IFileRemover
{
    private readonly IViewResolver viewResolver;
    public ObservableCollection<LoadedFileInfo> LoadedFiles { get; } = new();
    public ICommand AddFile { get; }
    public DelegateCommand NextStep { get; }

    public OpenFilesViewModel(IViewResolver viewResolver)
    {
        this.viewResolver = viewResolver;
        AddFile = new DelegateCommand(OnAddFile);
        NextStep = new DelegateCommand(OnNextStep) { IsEnabled = false };


        // test
        string path1 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\test.csv";
        LoadedFiles.Add(new LoadedFileInfo(path1, ParserChooser.ParseFile(path1), this));
        string path2 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\history_csv_20230126_160849.csv";
        LoadedFiles.Add(new LoadedFileInfo(path1, ParserChooser.ParseFile(path2), this));
        NextStep.IsEnabled = true;
    }

    private void OnAddFile()
    {
        OpenFileDialog fileDialog = new()
        {
            Filter = "csv|*.csv"
        };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            string fullPath = fileDialog.FileName;
            if (LoadedFiles.Any(f => f.FullPath == fullPath))
            {
                MessageBox.Show("File already added");
                return;
            }

            AccountFile loadedFile = ParserChooser.ParseFile(fullPath);
            LoadedFiles.Add(new LoadedFileInfo(fullPath, loadedFile, this));
            NextStep.IsEnabled = true;
        }
    }

    private void OnNextStep()
    {
        AccountFile[] accountFiles = LoadedFiles.Select(f => f.AccountFile).ToArray();
        OpenWindowHelper.OpenMainWindow(new MainWindowModel(
            viewResolver,
            new FilesSortingViewModel(viewResolver, accountFiles, new List<CategoryVm>
            {
                new () { Name = "Транспорт", Description = "qwe1"},
                new () { Name = "Подарки", Description = "asd1"}
            })));
    }

    public void RemoveFile(LoadedFileInfo file)
    {
        LoadedFiles.Remove(file);
        NextStep.IsEnabled = LoadedFiles.Count > 0;
    }
}

interface IFileRemover
{
    void RemoveFile(LoadedFileInfo file);
}