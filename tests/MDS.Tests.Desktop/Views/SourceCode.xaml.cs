using CommunityToolkit.Mvvm.ComponentModel;

using MDS.Tests.Desktop.ViewModels;

using Microsoft.UI.Xaml.Controls;

using Monaco;

namespace MDS.Tests.Desktop.Views;

[ObservableObject]
public sealed partial class SourceCode : Page
{
    public bool IsLoaded => MonacoEditor.IsLoaded;

    private SourceCodeViewModel? _viewModel;

    public SourceCodeViewModel? ViewModel
    {
        get => _viewModel ?? (ViewModel = App.GetService<SourceCodeViewModel>());
        private set
        {
            if (_viewModel is not null)
            {
                _viewModel.SourceCode = MonacoEditor.EditorContent;
            }

            if (SetProperty(ref _viewModel, value))
            {
                if (_viewModel is null)
                {
                    return;
                }

                _viewModel.RequestLoadedState += () => MonacoEditor.IsLoaded;

                if (_viewModel is not null)
                {
                    MonacoEditor.LoadContent(_viewModel.SourceCode, _viewModel.Language);
                }
            }
        }
    }

    public SourceCode()
    {
        InitializeComponent();
        ViewModel = App.GetService<SourceCodeViewModel>();
    }

    public async Task ChangeViewModel(SourceCodeViewModel newViewModel)
        => ViewModel = newViewModel;
}
