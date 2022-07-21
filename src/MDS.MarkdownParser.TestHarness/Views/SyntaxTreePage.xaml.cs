using MDS.MarkdownParser.TestHarness.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace MDS.MarkdownParser.TestHarness.Views;

public sealed partial class SyntaxTreePage : Page
{
    public SyntaxTreeViewModel ViewModel
    {
        get;
    }

    public SyntaxTreePage()
    {
        ViewModel = App.GetService<SyntaxTreeViewModel>();
        InitializeComponent();
    }
}
