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
        viewResolver.RegisterView<FileSortingView, FileSortingViewModel>(() => new FileSortingView());
        viewResolver.RegisterView<FilesSortingView, FilesSortingViewModel>(() => new FilesSortingView());
        viewResolver.RegisterView<CategoriesView, CategoriesViewModel>(() => new CategoriesView());
        viewResolver.RegisterView<HistoryView, HistoryViewModel>(() => new HistoryView());
        viewResolver.RegisterWindow<PkoBlockedOperationsWindow, PkoBlockedOperationParserVM>(() => new PkoBlockedOperationsWindow());
        DataContext = new OpenFilesViewModel(viewResolver);
    }
}