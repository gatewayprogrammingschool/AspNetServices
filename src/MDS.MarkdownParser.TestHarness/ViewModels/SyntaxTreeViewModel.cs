using System.Collections.ObjectModel;
using System.IO;

using Antlr4.Runtime;

using CommunityToolkit.Mvvm.ComponentModel;

namespace MDS.MarkdownParser.TestHarness.ViewModels;

public partial class SyntaxTreeViewModel : ObservableRecipient
{
    private const string FILENAME =
        "C:\\GitHub\\MarkdownServer\\AspNetServices\\tests\\MDS.TestSite\\wwwroot\\payton_byrd_timeline.md";

    public ObservableCollection<ParserRuleContextWrapper> ParseTree
    {
        get;
    }
        = new(new[] { GetParseTree(FILENAME),});

    private static ParserRuleContextWrapper GetParseTree(string filename = FILENAME)
    {
        string input = File.ReadAllText(filename);
        ICharStream stream = new AntlrInputStream(input);
        MDS.Antlr4md.MarkdownLexer lexer = new(stream);
        CommonTokenStream tokens = new(lexer);
        MDS.Antlr4md.MarkdownParser parser = new(tokens)
        {
            BuildParseTree = true,
        };

        return new(parser.md());
    }

    [ObservableProperty]
    private ParserRuleContextWrapper _selected;
}