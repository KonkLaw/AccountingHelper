using System;
using System.Windows.Input;

namespace AccountHelperWpf.Common;

class DelegateCommand : ICommand
{
    private bool isEnabled = true;
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (isEnabled == value)
                return;
            isEnabled = value;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private readonly Action action;

    public DelegateCommand(Action action) => this.action = action;

    public bool CanExecute(object? parameter) => isEnabled;

    public void Execute(object? parameter) => action();

    public event EventHandler? CanExecuteChanged;
}