using System.Windows;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // init
        AccountFile accountFile = PriorParser.ParseFile(@"C:\Users\Admin\Desktop\test.csv");
        AccountFile[] files = { accountFile };

        DataContext = new TableViewModel(files);
    }
}