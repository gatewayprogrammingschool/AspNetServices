using System.Text.RegularExpressions;

using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
// ReSharper disable IdentifierTypo

namespace MDS.AspnetServices;

internal static class MarkdownProcessor
{
    private static MarkdownPipeline? _pipeline;
    private static MarkdownPipeline Pipeline => _pipeline ??=
        MarkdownServerOptions.Current!.Services.GetService<MarkdownPipeline>()!;
    public static IResult ProcessContentFile(this MarkdownServerOptions options, string filename)
        => new Common.ContentResult(File.ReadAllText(filename));

    public static IResult ProcessFile(this MarkdownServerOptions options, string filename)
        => File.Exists(filename)
            ? options.ProcessContentFile(filename)
            : new MarkdownResponse(HttpStatusCode.NotFound).ToMarkdownResult();

    public static async Task<IResult> ProcessMarkdownFile(this MarkdownServerOptions options, string filename, ConcurrentDictionary<string, string>? variables = null)
        => File.Exists(filename)
            ? (await options.ProcessMarkdown(File.ReadAllText(filename), Path.GetDirectoryName(filename), variables)).ToResult()
            : new MarkdownResponse(HttpStatusCode.NotFound).ToMarkdownResult();

    // <([^>]*)md-include="([^"\n]*)"([^>]*)>
    public static async Task<string> ProcessHtmlIncludes(string html, ConcurrentDictionary<string, string>? variables=null)
    {
        var sb = new StringBuilder(html);
        var regex = new Regex(
            @"<([^>]*)md-include=(""([^""\n]*)""|'([^'\n]*)')([^>]*)>",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        var matches = regex.Matches(sb.ToString()).Cast<Match>().ToList();
        var enumerator = matches.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var match = enumerator.Current;

            var insertAt = match.Index + match.Length;

            var capture = match.Groups.OfType<Capture>().Last(c => c.Length > 0);

            var filename = capture.Value.Trim('"');

            if (filename is null or "")
            {
                continue;
            }

            filename = filename.Replace("/", "\\").Trim('\\');

            var root = MarkdownServerOptions.Current!.ServerRoot!;
            filename = Path.Combine(root, filename);

            if (filename is not null or "")
            {
                if (File.Exists(filename))
                {
                    var md = await MarkdownServerOptions.Current.ProcessMarkdown(
                        await File.ReadAllTextAsync(filename), root, variables);

                    var nestedHtml = Markdown.ToHtml(md,
                        MarkdownServerOptions.Current.Services.GetService<MarkdownPipeline>());

                    sb.Insert(insertAt, nestedHtml);
                }

                sb.Remove(capture.Index, capture.Length);
            }

            matches = regex.Matches(sb.ToString()).Cast<Match>().ToList();
            enumerator = matches.GetEnumerator();
        }

        return sb.ToString();
    }

    public static async Task<(string, ConcurrentDictionary<string, string>)>
        ProcessMarkdownIncludes(string markdown, MarkdownDocument originalDocument)
    {
        var sb = new StringBuilder(markdown);
        var regex = new Regex(
            @"^#include\(([^)\n]+)\)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        var matches = regex.Matches(sb.ToString()).Cast<Match>().ToList();
        var enumerator = matches.GetEnumerator();

        ConcurrentDictionary<string, string> variables = new();

        if (enumerator.MoveNext())
        {
            var match = enumerator.Current;

            var insertAt = match.Index;

            var capture = match.Groups.OfType<Capture>().Last(c => c.Length > 0);

            var filename = capture.Value.Trim('"');

            if (filename is not (null or ""))
            {
                string? md = null;

                var isHttp = filename.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);

                var root = MarkdownServerOptions.Current!.ServerRoot!;

                if (isHttp)
                {
                    var httpClient = MarkdownServerOptions.Current.Services.GetRequiredService<HttpClient>();

                    md = await httpClient.GetStringAsync(filename);
                }
                else
                {
                    var include = filename;

                    filename = Path.Combine(root, filename);

                    if (filename is not null or "" && File.Exists(filename))
                    {
                        md = await File.ReadAllTextAsync(filename);
                    }
                    else
                    {
                        return (markdown, variables);
                    }
                }

                var doc = await ProcessMarkdown(MarkdownServerOptions.Current, md, root);

                var vars = doc.GetData("Variables");

                if (vars is ConcurrentDictionary<string, string> nestedVars)
                {
                    foreach (var (key, value) in nestedVars)
                    {
                        variables.AddOrUpdate($"{capture.Value.Trim('"')}", value, (_, _) => value);
                    }
                }

                doc = Markdown.Parse(md, Pipeline);
                var docEnum = doc.GetEnumerator();
                if (docEnum.MoveNext() && docEnum.Current is ThematicBreakBlock tbb)
                {
                    if (docEnum.MoveNext())
                    {
                        var endIndex = docEnum.Current.Span.End;

                        md = md.Remove(0, endIndex).TrimStart();
                    }
                }

                sb.Remove(match.Index, match.Length);
                sb.Insert(insertAt, md);

                var nested = await ProcessMarkdownIncludes(sb.ToString(), originalDocument);

                if (nested.Item1 is not (null or ""))
                {
                    sb.Clear();
                    sb.Append(nested.Item1);
                }

                if (nested.Item2 is ConcurrentDictionary<string, string> nestedV)
                {
                    foreach (var (key, value) in nestedV)
                    {
                        variables.AddOrUpdate($"{capture.Value.Trim('"')}", value, (_, _) => value);
                    }
                }
            }
        }

        return (sb.ToString(), variables);
    }

    public static string ProcessFormTags(string markdown)
    {
        var sb = new StringBuilder(markdown);
        var regex = new Regex(
            @"^(![/]?[^#(:\s)]+){1}((#[\w\d_]+){0,1}(\((,?({[\w\d_-}]+=[^}]+\})){1,}\))?(:([^\n]+)){0,1})?",
            RegexOptions.Multiline);

        var matches = regex.Matches(markdown).Cast<Match>();
        foreach (var match in matches)
        {
            var toReplace = markdown[match.Index..(match.Index + match.Length)];

            var captures1 = match.Groups.Cast<Group>()
                .SelectMany(g => g.Captures).OfType<Group>()
                .SelectMany(cc => cc.Captures)
                .ToList();

            var captures2 = captures1.OfType<Group>()
                .SelectMany(g => g.Captures.OfType<Capture>())
                .ToList();

            var captures = captures1.Union(captures2).Distinct().ToList();

            var tagType = captures
                .FirstOrDefault(c =>
                    c.Value.StartsWith("!") &&
                    c.Value.IndexOf("#") == -1 &&
                    c.Value.IndexOf("(") == -1 &&
                    c.Value.IndexOf(":") == -1)?
                .Value
                .TrimStart('!');

            var classes = new List<string>();

            if((tagType?.IndexOf(".", StringComparison.OrdinalIgnoreCase) ?? -1) > -1)
            {
                classes.AddRange(tagType.Split('.').Skip(1));
            }

            var tagId = captures
                .FirstOrDefault(c =>
                    c.Value.StartsWith("#") &&
                    c.Value.IndexOf("(") == -1 &&
                    c.Value.IndexOf(":") == -1)?
                .Value
                .TrimStart('#');

            var tagAttributes = captures
                .Where(c => new Regex(@"^[,]{0,1}\{[^=]+=.*\}").IsMatch(c.Value))
                .ToList();

            var htmlAttributes = tagAttributes
                .Select(c =>
                    (name: c.Value
                        .TrimStart(",{".ToCharArray())
                        .TrimEnd('}')
                        .Split('=').First(),
                    value: c.Value
                            .TrimStart(",{".ToCharArray())
                            .TrimEnd('}')
                            .Split('=').Last()))
                .Select(t => $"{t.name}='{t.value}'")
                .Distinct()
                .ToList();

            var classAttribute = htmlAttributes.Find(
                s => s.StartsWith("class=", StringComparison.InvariantCultureIgnoreCase));
            
            classAttribute ??= "class=''";

            classAttribute = classAttribute[0..^1];

            classAttribute += $"{string.Join(" ", classes)}'";

            tagType = tagType?.Split('.').First();

            var tagValue = captures
                .FirstOrDefault(c =>
                    c.Value.StartsWith(":"))?
                .Value
                .TrimStart(':')
                .Trim();

            var counter = 0;
            var formTag = tagType switch
            {
                "/form" => "</form>",
                "/nav" => "</nav>",
                "nav" =>
                    $"<nav id='{tagId ?? $"nav{counter}"}' {classAttribute} name='{tagId ?? $"nav{counter++}"}' {string.Join(' ', htmlAttributes)}>",
                "form" =>
                    $"<form id='{tagId ?? $"form{counter}"}' {classAttribute} name='{tagId ?? $"form{counter++}"}' {string.Join(' ', htmlAttributes)}>",
                "input" =>
                    $"<input id='{tagId ?? $"input{counter}"}' {classAttribute} name='{tagId ?? $"input{counter++}"}' {string.Join(' ', htmlAttributes)} value='{tagValue}'></input>",
                "button" =>
                    $"<button id='{tagId ?? $"button{counter}"}' {classAttribute} name='{tagId ?? $"button{counter++}"}' {string.Join(' ', htmlAttributes)} value='{tagValue}'>{tagValue}</button>",
                _ =>
                    $"<{tagType} id='{tagId ?? $"{tagType}{counter}"}' {classAttribute} name='{tagId ?? $"{tagType}{counter++}"}' {string.Join(' ', htmlAttributes)} value='{tagValue}'>{tagValue}</{tagType}>"
            };

            sb.Replace(toReplace, formTag);
        }

        return sb.ToString();
    }

    public static async Task<MarkdownDocument> ProcessMarkdown(this MarkdownServerOptions options, string markdown, string myRoot, ConcurrentDictionary<string, string>? variables = null)
    {
        if (variables is null)
        {
            variables = new ConcurrentDictionary<string, string>();
        }

        var document = Markdig.Markdown.Parse(markdown, MarkdownResponse.Pipeline);

        markdown = ProcessFormTags(markdown);
        var (md, vars) = await ProcessMarkdownIncludes(markdown, document);
        markdown = md;

        foreach (var (k, v) in vars)
        {
            variables.AddOrUpdate(k, v, (_, _) => v);
        }

        var targetDocument = Markdig.Markdown.Parse(markdown, MarkdownResponse.Pipeline);

        for (var i = 0; i < document.Count; ++i)
        {
            var block = document[i];

            if (i is not 0 || block is not ThematicBreakBlock tbb)
            {
                continue;
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

                var name = linestring.Split(':').First().Trim();
                var value = linestring.Split(':').Last().Trim();

                if (name == "ViewModel")
                {
                    continue;
                }

                if (name.Equals("Layout", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (File.Exists(value))
                    {
                        value = await File.ReadAllTextAsync(value);
                        document.SetData(name, value);
                    }
                }

                variables.AddOrUpdate(name, value, (_, _) => value);
            }

            markdown = markdown.Remove(0, document[i].Span.End);

            foreach (var (key, value) in variables)
            {
                switch (value)
                {
                    case string sv when (sv.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase)):
                        sv = Path.GetFullPath(Path.Combine(myRoot, sv));
                        var nestedMarkdown = await options.ProcessNestedMarkdownFile(targetDocument, sv);
                        var text =
                                nestedMarkdown.OfType<LeafBlock>()?
                                    .FirstOrDefault()?
                                    .Inline?
                                    .OfType<LiteralInline>()?
                                    .FirstOrDefault()?
                                    .Content
                                    .Text
                                ?? value
                                    ;

                        var nestedHtml = nestedMarkdown.ToHtml(options.Services.GetService<MarkdownPipeline>());

                        variables.AddOrUpdate(key, nestedHtml, (_, _) => nestedHtml);

                        var nestedVariables =
                            nestedMarkdown.GetData("Variables") as ConcurrentDictionary<string, string>;

                        if (nestedVariables is not null)
                        {
                            foreach (var (nkey, nvalue) in nestedVariables)
                            {
                                variables.AddOrUpdate($"{key}_{nkey}", nvalue, (_, _) => nvalue);
                            }
                        }

                        markdown = markdown.Replace($"$({key})", text);
                        break;

                    default:
                        markdown = markdown.Replace($"$({key})", value);
                        break;
                }
            }

            targetDocument = Markdown.Parse(markdown, MarkdownResponse.Pipeline);
        }

        targetDocument.SetData("Variables", variables);

        return targetDocument;
    }

    internal static Task<MarkdownDocument> ProcessNestedMarkdownFile(this MarkdownServerOptions options, MarkdownDocument parent,
        string filename)
        => File.Exists(filename)
            ? options.ProcessMarkdown(File.ReadAllText(filename), Path.GetDirectoryName(filename)!)
            : throw new FileNotFoundException("Could not located nested file.", filename);

    public static MarkdownResult ToResult(this MarkdownDocument document)
    {
        return new MarkdownResult(document);
    }
}