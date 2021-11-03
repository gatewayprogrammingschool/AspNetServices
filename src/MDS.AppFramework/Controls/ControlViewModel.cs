using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MDS.AppFramework.Controls;

public abstract record ControlViewModel : INotifyPropertyChanged
{
    protected bool Set<TValue>(ref TValue oldValue,
                               TValue newValue,
                               TValue defaultValue = default,
                               [CallerMemberName] string property = null)
    {
        if (oldValue.Equals(newValue))
        {
            return false;
        }

        oldValue = newValue;
        OnPropertyChanged(property);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string property = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}