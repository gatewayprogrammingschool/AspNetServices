using MDS.Tests.Desktop.Activation;
using MDS.Tests.Desktop.Contracts.Services;
using MDS.Tests.Desktop.Core.Contracts.Services;
using MDS.Tests.Desktop.Core.Services;
using MDS.Tests.Desktop.Helpers;
using MDS.Tests.Desktop.Models;
using MDS.Tests.Desktop.Services;
using MDS.Tests.Desktop.ViewModels;
using MDS.Tests.Desktop.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MDS.Tests.Desktop;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static Page GetPage(Type type)
    {
        if ((App.Current as App)!.Host.Services.GetService(type) is not Page page)
        {
            throw new ArgumentException($"{type} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return page;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureAppConfiguration(configBuilder => 
            {
                configBuilder.AddJsonFile("appsettings.Desktop.json", false);
            }).
            ConfigureLogging(loggingBuilder=>
            {
                loggingBuilder.AddSimpleConsole();
            }).
            ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers

                // Services
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<IWebViewService, WebViewService>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IBlazorService, BlazorService>();
                services.AddSingleton<IConsoleService, ConsoleService>();

                // Core Services
                services.AddSingleton<IFileService, FileService>();

                // Views and ViewModels
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainPage>();
                services.AddTransient<ShellPage>();
                services.AddTransient<ShellViewModel>();

                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            }).
            Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    public static void Dispatch(Action action)
    {
        if (MainWindow.DispatcherQueue.HasThreadAccess)
        {
            action();
        }
        else
        {
            DispatcherQueueHandler handler = new(action);
            MainWindow.DispatcherQueue.TryEnqueue(
                DispatcherQueuePriority.High, 
                handler);
        }
    }
}
