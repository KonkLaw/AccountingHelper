using AccountHelperWpf.Common;
using AccountHelperWpf.ViewModels;
using System.Windows;

namespace AccountHelperWpf.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        ViewResolver viewResolver = new ();
        DataContext = new MainWindowModel(viewResolver);;
    }
}