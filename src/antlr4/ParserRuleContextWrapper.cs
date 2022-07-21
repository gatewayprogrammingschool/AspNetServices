namespace MDS.Antlr4md;

using Antlr4.Runtime;

using System.Collections.ObjectModel;

public record ParserRuleContextWrapper(ParserRuleContext? Inner)
{
    private const string NULL = "<<null>>";

    public string TypeName => Inner?.GetType().Name ?? NULL;
    public string Text => Inner?.GetText() ?? NULL;

    public string Start => $"{Inner?.Start?.Line}:{Inner?.Start?.Column}";
    public string End => $"{Inner?.Stop?.Line}:{Inner?.Stop?.Column}";
    public string Span => $"{Start} -> {End}";

    public string InnerString => Inner?.ToString() ?? NULL;

    public string[] TokenList
        => Inner?.GetTokens(Inner?.RuleIndex ?? 0)
                 .Select(n => n?.ToString() ?? NULL)
                 .ToArray()
            ?? Array.Empty<string>();

    public string Tokens => string.Join(", ", TokenList);

    public ObservableCollection<ParserRuleContextWrapper> Children { get; } =
        (Inner?.exception is null)
        ? new ObservableCollection<ParserRuleContextWrapper>(Inner?.children?
             .Select(i => new ParserRuleContextWrapper(i as ParserRuleContext))
            ?? Array.Empty<ParserRuleContextWrapper>())
        : new(Array.Empty<ParserRuleContextWrapper>());

    public string Exception => Inner?.exception?.ToString() ?? NULL;


    public ParserRuleContextWrapper? Parent
        => new (Inner?.Parent as ParserRuleContext);
}
