using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MDS.Tests.Desktop.Contracts.Services;
using MDS.Tests.Desktop.Contracts.ViewModels;
using MDS.Tests.Desktop.Core.Contracts.Services;

using Microsoft.Web.WebView2.Core;

namespace MDS.Tests.Desktop.ViewModels;

// TODO: Review best practices and distribution guidelines for WebView2.
// https://docs.microsoft.com/microsoft-edge/webview2/get-started/winui
// https://docs.microsoft.com/microsoft-edge/webview2/concepts/developer-guide
// https://docs.microsoft.com/microsoft-edge/webview2/concepts/distribution
public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    // TODO: Set the default URL to display.
    [ObservableProperty]
    private Uri source = new("http://localhost:5001/legal/");

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private bool hasFailures;

    [ObservableProperty]
    private string webpageTitle;

    public IWebViewService WebViewService
    {
        get;
    }
    public IConsoleService ConsoleService
    {
        get;
    }

    public MainViewModel(IWebViewService webViewService, IConsoleService consoleService)
    {
        WebViewService = webViewService;
        ConsoleService = consoleService;

        ConsoleService.PropertyChanged += ConsoleService_PropertyChanged;
    }

    public string STDOUT => ConsoleService.STDOUT;
    public string STDERR => ConsoleService.STDERR;

    private void ConsoleService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ConsoleService.STDOUT):
                OnPropertyChanged(nameof(STDOUT));
                break;
            case nameof(ConsoleService.STDERR):
                OnPropertyChanged(nameof(STDERR));
                break;
        }
    }

    [RelayCommand]
    private async Task OpenInBrowser()
    {
        if (WebViewService.Source != null)
        {
            await Windows.System.Launcher.LaunchUriAsync(WebViewService.Source);
        }
    }

    [RelayCommand]
    private void Reload()
    {
        WebViewService.Reload();
    }

    [RelayCommand]
    private void Print()
    {
        WebViewService.Print();
    }

    [RelayCommand(CanExecute = nameof(BrowserCanGoForward))]
    private void BrowserForward()
    {
        if (WebViewService.CanGoForward)
        {
            WebViewService.GoForward();
        }
    }

    private bool BrowserCanGoForward()
    {
        return WebViewService.CanGoForward;
    }

    [RelayCommand(CanExecute = nameof(BrowserCanGoBack))]
    private void BrowserBack()
    {
        if (WebViewService.CanGoBack)
        {
            WebViewService.GoBack();
        }
    }

    private bool BrowserCanGoBack()
    {
        return WebViewService.CanGoBack;
    }

    public void OnNavigatedTo(object parameter)
    {
        WebViewService.NavigationCompleted += OnNavigationCompleted;
        WebViewService.StatusChanged += OnStatusChanged;
    }

    private void OnStatusChanged(object? sender, string? e)
        => WebpageTitle = e ?? "<null>"; 

    public void OnNavigatedFrom()
    {
        WebViewService.UnregisterEvents();
        WebViewService.NavigationCompleted -= OnNavigationCompleted;
        WebViewService.StatusChanged -= OnStatusChanged;
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs webErrorStatus)
    {
        IsLoading = false;
        BrowserBackCommand.NotifyCanExecuteChanged();
        BrowserForwardCommand.NotifyCanExecuteChanged();

        if (webErrorStatus.WebErrorStatus != default)
        {
            HasFailures = true;
        }
    }

    [RelayCommand]
    private void OnRetry()
    {
        HasFailures = false;
        IsLoading = true;
        WebViewService?.Reload();
    }
}
