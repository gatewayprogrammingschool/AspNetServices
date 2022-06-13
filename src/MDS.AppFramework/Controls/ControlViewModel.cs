using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Primitives;

namespace MDS.AppFramework.Controls;

public abstract record ControlViewModel : IFormCollection, INotifyPropertyChanged
{
    protected ControlViewModel(IFormCollection collection) : this()
    {
        foreach(var kvp in collection)
        {
            Values.TryAdd(kvp.Key, kvp.Value);
        }
    }

    protected ControlViewModel() { }

    protected ConcurrentDictionary<string, StringValues> Values = new();

    public int Count { get; }
    public ICollection<string> Keys => Values.Keys;
    public IFormFileCollection Files { get; } = new FormFileCollection();

    public StringValues this[string key] => Values[key];

    public bool Set(string key, object value)
    {
        if (value is StringValues sv)
        {
            bool result = Values.AddOrUpdate(key, sv, (_, _) => sv).Equals(sv);

            if (result)
            {
                OnPropertyChanged(key);
            }

            return result;
        }

        return false;
    }

    public object? Get(string key)
        => Values.TryGetValue(key, out StringValues value) ? value : default;

    protected bool Set<TValue>(ref TValue? oldValue,
                               TValue? newValue,
                               TValue? defaultValue = default,
                               [CallerMemberName] string? property = null)
    {
        if (oldValue is StringValues && newValue is StringValues)
        {
            if (oldValue?.Equals(newValue) ?? newValue is not null)
            {
                return false;
            }

            oldValue = newValue;
            OnPropertyChanged(property);
            return true;
        }

        return false;
    }

    protected void OnPropertyChanged([CallerMemberName] string? property = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public bool ContainsKey(string key)
    {
        return Keys.Contains(key);
    }

    public bool TryGetValue(string key, out StringValues value)
    {
        return Values.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}