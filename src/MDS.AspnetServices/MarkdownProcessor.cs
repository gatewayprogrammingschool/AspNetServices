using Markdig.Syntax;

namespace MDS.AspnetServices;

internal static class MarkdownProcessor
{
    public static IResult ProcessContentFile(this MarkdownServerOptions options, string filename) 
        => new Common.ContentResult(File.ReadAllText(filename));

    public static IResult ProcessFile(this MarkdownServerOptions options, string filename) 
        => File.Exists(filename) 
            ? options.ProcessContentFile(filename)
            : new MarkdownResponse(HttpStatusCode.NotFound).ToMarkdownResult();

    public static IResult ProcessMarkdownFile(this MarkdownServerOptions options, string filename) 
        => File.Exists(filename) 
            ? options.ProcessMarkdown(File.ReadAllText(filename)).ToResult() 
            : new MarkdownResponse(HttpStatusCode.NotFound).ToMarkdownResult();

    public static MarkdownDocument ProcessMarkdown(this MarkdownServerOptions options, string markdown)
    {
        var variables = new ConcurrentDictionary<string, string>();
        var document = Markdig.Markdown.Parse(markdown, MarkdownResponse.Pipeline);

        var targetDocument = Markdig.Markdown.Parse(markdown, MarkdownResponse.Pipeline);

        for(var i = 0; i < document.Count; ++i)
        {
            var block = document[i];

            if (i is not 0 || block is not ThematicBreakBlock tbb)
            {
                break;
            }

            ++i;

            var frontMatter = markdown[document[i].Span.Start..document[i].Span.End]
                .Split(Environment.NewLine);

            if (frontMatter.Length == 1)
            {
                frontMatter = frontMatter[0].Split('\n');
            }

            // YAML Front Matter
            foreach (var line in frontMatter)
            {
                var linestring = line.ToString();
                    
                if (linestring.IndexOf(":", StringComparison.OrdinalIgnoreCase) <= -1)
                {
                    continue;
                }

                var name = linestring.Split(':').First();
                var value = linestring.Split(':').Last();

                variables.AddOrUpdate(name, value, (_, _) => value);
            }

            markdown = markdown.Remove(0, document[i].Span.End);
                
            foreach (var (key, value) in variables)
            {
                markdown = markdown.Replace($"$({key})", value);
            }

            targetDocument = Markdown.Parse(markdown, MarkdownResponse.Pipeline);
        }

        targetDocument.SetData("Variables", variables);

        return targetDocument;
    }

    public static MarkdownResult ToResult(this MarkdownDocument document)
    {
        return new MarkdownResult(document);
    }
}