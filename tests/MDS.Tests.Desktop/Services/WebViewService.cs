using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.WinUI.Helpers;

using MDS.Tests.Desktop.Contracts.Services;
using MDS.Tests.Desktop.Core.Contracts.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

using Windows.Graphics.Printing;

using Windows.UI.Popups;

namespace MDS.Tests.Desktop.Services;

public class WebViewService : IWebViewService
{
    private WebView2? _webView;

    public Uri? Source => _webView?.Source;

    [MemberNotNullWhen(true, nameof(_webView))]
    public bool CanGoBack => _webView != null && _webView.CanGoBack;

    [MemberNotNullWhen(true, nameof(_webView))]
    public bool CanGoForward => _webView != null && _webView.CanGoForward;

    public IBlazorService BlazorService
    {
        get;
    }

    public event EventHandler<CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;
    public event EventHandler<string?>? StatusChanged;

    public WebViewService(IBlazorService blazorService)
    {
        BlazorService = blazorService;
    }

    [MemberNotNull(nameof(_webView))]
    public void Initialize(WebView2? webView)
    {
        if (webView is null)
        {
            throw new ArgumentNullException(nameof(webView));
        }

        _webView = webView;
        _webView.NavigationCompleted += OnWebViewNavigationCompleted;
        _webView.CoreWebView2Initialized += (o,e) 
            => _webView.CoreWebView2.DocumentTitleChanged += DocumentTitleChanged;
        

        BlazorService.Initialize();

    }

    private void DocumentTitleChanged(CoreWebView2 sender, object args)
    {
        StatusChanged?.Invoke(sender, _webView.CoreWebView2.DocumentTitle);
    }
    public void GoBack() => _webView?.GoBack();

    public void GoForward() => _webView?.GoForward();

    public void Reload() => _webView?.Reload();

    public async Task Print()
    {
        _webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);

        //// Create a new PrintHelperOptions instance
        //var defaultPrintHelperOptions = new PrintHelperOptions();

        //// Configure options that you want to be displayed on the print dialog
        //defaultPrintHelperOptions.AddDisplayOption(StandardPrintTaskOptions.Orientation);
        //defaultPrintHelperOptions.Orientation = PrintOrientation.Portrait;

        //// Create a new PrintHelper instance
        //// "container" is a XAML panel that will be used to host printable control.
        //// It needs to be in your visual tree but can be hidden with Opacity = 0
        //var printHelper = new PrintHelper((Panel)_webView.Parent, defaultPrintHelperOptions);

        //// Add controls that you want to print
        //printHelper.AddFrameworkElementToPrint(_webView);

        //// And/Or create pages programmatically
        //var grid = new Grid();
        //grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        //grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        //var header = new TextBlock { Text = "This is the header", Margin = new Thickness(0, 0, 0, 20) };
        //Grid.SetRow(header, 0);
        //grid.Children.Add(header);
        //var content = _webView;
        //Grid.SetRow(content, 1);
        //grid.Children.Add(content);
        //printHelper.AddFrameworkElementToPrint(grid);

        //// Connect to relevant events
        //printHelper.OnPrintFailed += PrintHelper_OnPrintFailed;
        //printHelper.OnPrintSucceeded += PrintHelper_OnPrintSucceeded;

        //// Start printing process
        //await printHelper.ShowPrintUIAsync("Windows Community Toolkit Sample App");

        //// Event handlers

        //async void PrintHelper_OnPrintSucceeded()
        //{
        //    printHelper.Dispose();
        //    var dialog = new MessageDialog("Printing done.");
        //    await dialog.ShowAsync();
        //}

        //async void PrintHelper_OnPrintFailed()
        //{
        //    printHelper.Dispose();
        //    var dialog = new MessageDialog("Printing failed.");
        //    await dialog.ShowAsync();
        //}
    }

    public void UnregisterEvents()
    {
        if (_webView != null)
        {
            _webView.NavigationCompleted -= OnWebViewNavigationCompleted;
        }
    }

    private void OnWebViewNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args) 
        => NavigationCompleted?.Invoke(this, args);

    public void NavigateWebViewTo(Uri uri)
    {
        if (_webView != null)
        {
            _webView.Source = uri;
        }
    }
}
