using CommunityToolkit.Mvvm.ComponentModel;

namespace MDS.Tests.Desktop.ViewModels;

public partial class SourceCodeViewModel : ObservableRecipient
{
    public event Func<bool> RequestLoadedState;

    public bool GetLoadedState() => RequestLoadedState?.Invoke() ?? true;

    [ObservableProperty]
    private string _sourceCode;

    [ObservableProperty]
    private string _language;

    public SourceCodeViewModel()
    {
    }
}
