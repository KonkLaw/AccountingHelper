using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccountHelperWpf.ViewUtils;

class BaseNotifyProperty : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var qwe = PropertyChanged?.GetInvocationList().Length;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T source, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(source, newValue))
            return false;
        source = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }
}