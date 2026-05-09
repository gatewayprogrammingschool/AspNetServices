using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MDS.Tests.Desktop.Models;

using Monaco;

namespace MDS.Tests.Desktop.Contracts.Services;

public interface IMonacoService
{
    void OpenSourceCommand(NavLink navLink);
    void CloseSourceCommand(NavLink navLink);

    NavLink CurrentSource
    {
        get;
        set;
    }

    Monaco.MonacoEditor Editor
    {
        get; set;
    }
}

public partial class MonacoService : ObservableObject//, IMonacoService
{
    [ObservableProperty]
    private NavLink _currentSource;

    [ObservableProperty]
    private MonacoEditor _editor;

    [RelayCommand]
    private void CloseSource(NavLink navLink)
    {
        //_editor
    }

    [RelayCommand]
    private void OpenSource(NavLink navLink)
    {
    }
}