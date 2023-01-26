using System.Windows;
using AccountHelperWpf.Common;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Views;
/// <summary>
/// Interaction logic for OpenFiles.xaml
/// </summary>
public partial class OpenFilesWindow : Window
{
    public OpenFilesWindow()
    {
        InitializeComponent();
        ViewResolver viewResolver = new ();
        viewResolver.RegisterViewModel<FileSortingView, FileSortingViewModel>(() => new FileSortingView());
        viewResolver.RegisterViewModel<FilesSortingView, FilesSortingViewModel>(() => new FilesSortingView());
        viewResolver.RegisterViewModel<CategoriesView, CategoriesViewModel>(() => new CategoriesView());
        viewResolver.RegisterViewModel<HistoryView, HistoryViewModel>(() => new HistoryView());
        DataContext = new OpenFilesViewModel(viewResolver);
    }
}