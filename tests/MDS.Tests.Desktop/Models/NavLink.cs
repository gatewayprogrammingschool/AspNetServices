using System.Collections.ObjectModel;

using Microsoft.UI.Xaml.Controls;

namespace MDS.Tests.Desktop.Models;

public class NavLink
{
    public string Label
    {
        get; set;
    }
    public Symbol Symbol
    {
        get; set;
    }

    public ObservableCollection<NavLink> Children
    {
        get;
    } = new();

    public string Path
    {
        get;set;
    }

    public string FullPath
    {
        get;set;
    }

    public double Margin
    {
        get;
        set;
    } = 0;

    public string MarginString => $"{Margin},0,0,0";

    public bool CanDelete
    {
        get; set;
    } = true;
}
