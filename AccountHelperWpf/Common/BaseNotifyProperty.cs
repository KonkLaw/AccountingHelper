using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccountHelperWpf.Common;

class BaseNotifyProperty : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T source, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(source, newValue))
            return false;
        source = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }
}