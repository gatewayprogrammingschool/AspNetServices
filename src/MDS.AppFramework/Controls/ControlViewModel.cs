using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MDS.AppFramework.Controls;

public abstract record ControlViewModel : INotifyPropertyChanged
{
    protected ConcurrentDictionary<string, object> Values = new();

    public bool Set(string key, object value)
    {
        var result = Values.AddOrUpdate(key, value, (_,_) => value).Equals(value);

        if (result)
        {
            OnPropertyChanged(key);
        }

        return result;
    }

    public object? Get(string key)
        => Values.TryGetValue(key, out var value) ? value : null;

    protected bool Set<TValue>(ref TValue? oldValue,
                               TValue? newValue,
                               TValue? defaultValue = default,
                               [CallerMemberName] string? property = null)
    {
        if (oldValue?.Equals(newValue) ?? newValue is not null)
        {
            return false;
        }

        oldValue = newValue;
        OnPropertyChanged(property);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? property = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}