using System.Windows;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Views;

partial class Start : Window
{
    public Start()
    {
        InitializeComponent();
        DataContext = new StartVM();
    }
}