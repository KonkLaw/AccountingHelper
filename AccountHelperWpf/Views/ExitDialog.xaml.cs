using System.Windows;

namespace AccountHelperWpf.Views;

/// <summary>
/// Interaction logic for ExitDialog.xaml
/// </summary>
partial class ExitDialog : Window
{
    internal ExitState? Result { get; private set; }

    public ExitDialog() => InitializeComponent();

    private void SaveAndExit_OnClick(object sender, RoutedEventArgs e) => SetAndExit(ExitState.SaveAndExit);

    private void Exit_OnClick(object sender, RoutedEventArgs e) => SetAndExit(ExitState.ExitNoSave);

    private void Cancel_OnClick(object sender, RoutedEventArgs e) => SetAndExit(ExitState.Cancel);

    private void SetAndExit(ExitState newState)
    {
        Result = newState;
        Close();
    }
}

internal enum ExitState
{
    SaveAndExit,
    ExitNoSave,
    Cancel,
}