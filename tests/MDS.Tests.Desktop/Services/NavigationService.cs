using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Timers;

using MDS.Tests.Desktop.Contracts.Services;
using MDS.Tests.Desktop.Contracts.ViewModels;
using MDS.Tests.Desktop.Helpers;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using Windows.UI.Core;

using Timer = System.Timers.Timer;

namespace MDS.Tests.Desktop.Services;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public class NavigationService : INavigationService
{
    private readonly IPageService _pageService;
    private object? _lastParameterUsed;
    private Frame? _frame;

    public event EventHandler<object?>? Navigated;

    private readonly ConcurrentDictionary<string, Page> _pages = new();
    private readonly ConcurrentStack<string> _backStack = new();
    private readonly ConcurrentStack<string> _forwardStack = new();
    private Timer? _timer;

    public Frame? Frame
    {
        get
        {
            if (_frame == null)
            {
                _frame = App.MainWindow.Content as Frame;
                RegisterFrameEvents();
            }

            return _frame;
        }

        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    public bool CanGoBack => _backStack.Any();

    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }

    private void RegisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated += OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated -= OnNavigated;
        }
    }

    public bool GoBack()
    {
        if (CanGoBack && _backStack.TryPop(out var key))
        {
            var vmBeforeNavigation = Frame.GetPageViewModel();

            if (vmBeforeNavigation is not null)
            {
                _forwardStack.Push(vmBeforeNavigation.GetType().Name);
            }

            if (_pages.TryGetValue(key, out var page))
            {
                Frame.Content = page;

                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }

                _timer = new Timer(TimeSpan.FromSeconds(0.5));

                _timer.Elapsed += _timer_Elapsed;

                _timer.Start();

                return true;
            }
        }

        return false;
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var pageType = _pageService.GetPageType(pageKey);

        if (!_pages.TryGetValue(pageKey, out Page? page))
        {
            page = App.GetPage(pageType);

            if (page is not null)
            {
                _pages.TryAdd(pageKey, page);
            }
        }

        if (page is not null && _frame != null && (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed))))
        {
            _frame.Tag = clearNavigation;
            var vmBeforeNavigation = _frame.GetPageViewModel();

            if (vmBeforeNavigation is not null)
            {
                string toPush = vmBeforeNavigation.GetType().FullName!;
                _backStack.Push(toPush);
            }

            _frame.Content = page;

            _lastParameterUsed = parameter;
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }

            _timer = new Timer(TimeSpan.FromSeconds(0.5));

            _timer.Elapsed += _timer_Elapsed;

            _timer.Start();

            return true;
        }

        return false;
    }

    private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop();



        OnNavigated(_frame!, null);
    }

    private void OnNavigated(object sender, object? parameter) => App.Dispatch(() =>
                                                                       {
                                                                           if (sender is Frame frame)
                                                                           {
                                                                               var clearNavigation = (bool)frame.Tag;
                                                                               if (clearNavigation)
                                                                               {
                                                                                   frame.BackStack.Clear();
                                                                               }

                                                                               if (frame.GetPageViewModel() is INavigationAware navigationAware)
                                                                               {
                                                                                   navigationAware.OnNavigatedTo(parameter ?? new());
                                                                               }

                                                                               Navigated?.Invoke(sender, null);
                                                                           }
                                                                       });
}
