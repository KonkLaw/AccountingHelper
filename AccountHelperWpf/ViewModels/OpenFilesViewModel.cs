using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\test.csv";
        LoadedFiles.Add(new LoadedFileInfo(path, PriorParser.ParseFile(path), this));
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

            AccountFile loadedFile = PriorParser.ParseFile(fullPath);
            LoadedFiles.Add(new LoadedFileInfo(fullPath, loadedFile, this));
            NextStep.IsEnabled = true;
        }
    }

    private void OnNextStep()
    {
        AccountFile[] accountFiles = LoadedFiles.Select(f => f.AccountFile).ToArray();
        OpenWindowHelper.OpenMainWindow(new MainWindowModel(
            viewResolver,
            new FilesSortingViewModel(viewResolver, accountFiles)));
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