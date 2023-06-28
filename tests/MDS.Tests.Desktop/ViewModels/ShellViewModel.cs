using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MDS.Tests.Desktop.Contracts.Services;
using MDS.Tests.Desktop.Dialogs;
using MDS.Tests.Desktop.Models;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using Windows.UI.Popups;

namespace MDS.Tests.Desktop.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
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
    public NewDocumentViewModel NewDocViewModel
    {
        get;
    }

    [ObservableProperty]
    private bool _isDialogOpen = false;

    [RelayCommand]
    internal async Task DeleteFile(NavLink item)
        => await Confirm($"Are you sure you want to delete {item.Label}", () => File.Delete(item.FullPath));

    private async Task Confirm(string content, Action action)
    {
        var dlg = new ContentDialog()
        {
            Content = content,
            CloseButtonText = "No",
            PrimaryButtonText = "Yes",
            DefaultButton=ContentDialogButton.Primary,
        };

        dlg.XamlRoot = XamlRoot;

        var result = await dlg.ShowAsync(ContentDialogPlacement.UnconstrainedPopup);

        switch (result)
        {
            case ContentDialogResult.Primary:
                action();
                Initialize();
                break;
        }
    }

    [RelayCommand]
    private void CancelDialog()
    {
        IsDialogOpen = false;
        NewDocViewModel.Reset();
    }

    [ObservableProperty]
    private string _dialogToShow;

    [ObservableProperty]
    private XamlRoot _xamlRoot;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    private bool _isBackEnabled;

    [ObservableProperty]
    private bool _isPaneOpen = true;

    [RelayCommand]
    private void TogglePanel() => IsPaneOpen = !IsPaneOpen;

    [RelayCommand]
    private void Back()
    {
        if (!IsBackEnabled)
        {
            return;
        }

        NavigationService.GoBack();
    }

    [RelayCommand]
    private void MenuFileExit() => Application.Current.Exit();

    public event Action OpenNewDocDialog;

    [RelayCommand]
    private void MenuFileNew()
    {
        IsDialogOpen = false;
        DialogToShow = "NewDocumentDialog";
        OpenNewDocDialog?.Invoke();
    }

    [RelayCommand]
    private void MenuFileSave()
    {
    }

    [RelayCommand]
    private void MenuFileSaveAs()
    {
    }

    [RelayCommand]
    private void MenuFilePrint()
    {
    }

    [RelayCommand]
    private void MenuSettings() => NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);

    [RelayCommand]
    private void MenuViewsMain() => NavigationService.NavigateTo(typeof(MainViewModel).FullName!);

    [ObservableProperty]
    private ObservableCollection<NavLink> _navLinks = new();

    public ShellViewModel(
        INavigationService navigationService,
        IWebViewService webViewService,
        NewDocumentViewModel newDocViewModel)
    {
        NavigationService = navigationService;
        WebViewService = webViewService;
        NewDocViewModel = newDocViewModel;
        NewDocViewModel.CloseDialogRequested += () =>
        {
            Initialize();
            IsDialogOpen = false;
        };

        NavigationService.Navigated += OnNavigated;
    }

    public void Initialize()
    {
        NavLinks = new()
        {
            new(){ Label = "Home", Symbol=Symbol.Home, CanDelete=false },
            new(){ Label = "TOC", Symbol=Symbol.ShowResults, CanDelete=false },
            new(){ Label = "Items", Symbol=Symbol.Document, CanDelete=false },
            new(){ Label = "Index", Symbol=Symbol.AllApps, CanDelete=false }
        };

        var root = BlazorProgram.WebApp.Environment.WebRootPath;

        var documents = new List<string>(Directory.GetFiles(root, "*.md", SearchOption.AllDirectories));
        documents.AddRange(Directory.GetFiles(root, "*.yml", SearchOption.AllDirectories));
        documents.AddRange(Directory.GetFiles(root, "*.yaml", SearchOption.AllDirectories));
        documents.AddRange(Directory.GetFiles(root, "*.css", SearchOption.AllDirectories));
        documents.AddRange(Directory.GetFiles(root, "*.html", SearchOption.AllDirectories));

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
                        Path = folderPath,
                        FullPath = Path.Combine(root, folderPath),
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
                    ".css" => Symbol.FontColor,
                    ".html" => Symbol.Page,
                    _ => Symbol.Folder
                },
                Path = path,
                FullPath = doc,
                Margin = (segments.Length - 1) * 42,
            };

            NavLinks.Insert(NavLinks.Count - 1, item);
        }
    }

    private void OnNavigated(object? sender, object? e) => IsBackEnabled = NavigationService.CanGoBack;

    public void DocumentSelected(NavLink navLink)
    {
        if (navLink.Path is { Length: > 0 })
        {
            if (File.Exists(navLink.FullPath))
            {
                NavigationService.NavigateTo(typeof(MainViewModel).FullName);

                Uri uri = new($"http://localhost:5001/{navLink.Path.Trim('\\').Replace('\\', '/')}");
                WebViewService.NavigateWebViewTo(uri);
            }
        }
    }
}
