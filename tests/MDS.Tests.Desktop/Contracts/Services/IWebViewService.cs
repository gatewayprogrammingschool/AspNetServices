using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace MDS.Tests.Desktop.Contracts.Services;

public interface IWebViewService
{
    Uri? Source
    {
        get;
    }

    bool CanGoBack
    {
        get;
    }

    bool CanGoForward
    {
        get;
    }

    event EventHandler<CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;
    event EventHandler<string?>? StatusChanged;

    void Initialize(WebView2 webView);

    void GoBack();

    void GoForward();

    void Reload();

    Task Print();

    void UnregisterEvents();
    void NavigateWebViewTo(Uri uri);
}
