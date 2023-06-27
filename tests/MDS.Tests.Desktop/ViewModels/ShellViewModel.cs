using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MDS.Tests.Desktop.Contracts.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace MDS.Tests.Desktop.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    private bool isBackEnabled;

    public ICommand MenuFileExitCommand
    {
        get;
    }

    public ICommand MenuSettingsCommand
    {
        get;
    }

    public ICommand MenuViewsMainCommand
    {
        get;
    }

    public IRelayCommand BackCommand
    {
        get;
    }

    public IRelayCommand TogglePanelCommand
    {
        get;
    }

    public INavigationService NavigationService
    {
        get;
    }
    public MainViewModel MainViewModel
    {
        get;
    }
    public IWebViewService WebViewService
    {
        get;
    }

    [ObservableProperty]
    private bool isPaneOpen = true;

    private void OnTogglePanel()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    private void OnBack()
    {
        if (!IsBackEnabled)
        {
            return;
        }

        NavigationService.GoBack();
    }

    private void OnNavigated(object sender, object? e)
    {
        IsBackEnabled = NavigationService.CanGoBack;
    }

    private void OnMenuFileExit() => Application.Current.Exit();

    private void OnMenuSettings()
    {
        NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
    }

    private void OnMenuViewsMain()
    {
        NavigationService.NavigateTo(typeof(MainViewModel).FullName!);
    }

    public ObservableCollection<NavLink> NavLinks
    {
        get;
    } = new()
    {
        new(){ Label = "Home", Symbol=Symbol.Home },
        new(){ Label = "TOC", Symbol=Symbol.ShowResults },
        new(){ Label = "Items", Symbol=Symbol.Document },
        new(){ Label = "Index", Symbol=Symbol.AllApps }
    };

    public ShellViewModel(INavigationService navigationService, IWebViewService webViewService)
    {
        NavigationService = navigationService;
        WebViewService = webViewService;
        NavigationService.Navigated += OnNavigated;

        MenuFileExitCommand = new RelayCommand(OnMenuFileExit);
        MenuSettingsCommand = new RelayCommand(OnMenuSettings);
        MenuViewsMainCommand = new RelayCommand(OnMenuViewsMain);
        BackCommand = new RelayCommand(OnBack, () => IsBackEnabled);
        TogglePanelCommand = new RelayCommand(OnTogglePanel);

        var root = BlazorProgram.WebApp.Environment.WebRootPath;

        var documents = new List<string>(Directory.GetFiles(root, "*.md", SearchOption.AllDirectories));
        documents.AddRange(Directory.GetFiles(root, "*.yml"));
        documents.AddRange(Directory.GetFiles(root, "*.yaml"));

        var sorted = documents.Order().ToList();

        var rootNode = NavLinks[2];

        Dictionary<string, NavLink> folders = new();

        foreach (var doc in sorted)
        {
            var currentNode = rootNode;
            var path = doc.Replace(root, "");

            var segments = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            var folderPath = Path.Combine(segments[..^1]);

            var name = segments.Last();
            var extension = Path.GetExtension(name);

            if (folderPath is { Length: > 0 })
            {
                if (!NavLinks.Any(nl => nl.Label == folderPath))
                {
                    NavLink newNavLink = new()
                    {
                        Label = folderPath,
                        Symbol = Symbol.Folder,
                        Path = path,
                        FullPath = doc,
                        Margin = (segments.Length - 2) * 48,
                    };

                    NavLinks.Insert(NavLinks.Count - 1, newNavLink);
                }
            }

            var item = new NavLink
            {
                Label = name,
                Symbol = extension switch
                {
                    ".md" => Symbol.Document,
                    ".yml" or ".yaml" => Symbol.PreviewLink,
                    _ => Symbol.Folder
                },
                Path = path,
                FullPath = doc,
                Margin = (segments.Length - 1) * 42,
            };

            NavLinks.Insert(NavLinks.Count-1, item);
        }
    }

    public void DocumentSelected(NavLink navLink)
    {
        if (navLink.Path is { Length: > 0 })
        {
            NavigationService.NavigateTo(typeof(MainViewModel).FullName);

            Uri uri = new($"http://localhost:5001/{navLink.Path.Trim('\\').Replace('\\', '/')}");
            WebViewService.NavigateWebViewTo(uri);
        }
    }
}

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
}
