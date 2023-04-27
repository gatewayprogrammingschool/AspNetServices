using System.Text.RegularExpressions;

using Markdig.Syntax;
using Markdig.Syntax.Inlines;

using YamlDotNet.Serialization;

using ContentResult = MDS.AspnetServices.Common.ContentResult;
using MD = Markdig.Markdown;

// ReSharper disable RedundantAssignment

// ReSharper disable IdentifierTypo
// ReSharper disable ConvertTypeCheckPatternToNullCheck

namespace MDS.AspnetServices;

internal static class MarkdownProcessor
{
    private static MarkdownPipeline Pipeline => _pipeline
        ??= MarkdownServerOptions.Current!.Services.GetService<MarkdownPipeline>()!;

    public static IResult ProcessContentFile(string filename)
        => new ContentResult(File.ReadAllText(filename));

    // ReSharper disable once UnusedParameter.Global
    public static IResult ProcessFile(this MarkdownServerOptions options, string filename)
        => File.Exists(filename)
            ? MarkdownProcessor.ProcessContentFile(filename)
            : new MarkdownResponse(HttpStatusCode.NotFound).ToMarkdownResult();

    public static string ProcessFormTags(string markdown)
    {
        StringBuilder sb = new(markdown);
        Regex regex = new(
            @"^(![/]?[^#(:\s)]+){1}((#[\w\d_]+){0,1}(\((,?({[\w\d_-}]+=[^}]+\})){1,}\))?(:([^\n]+)){0,1})?",
            RegexOptions.Multiline
        );

        IEnumerable<Match> matches = regex.Matches(markdown);

        foreach (Match match in matches)
        {
            string toReplace = markdown[match.Index..(match.Index + match.Length)];

            List<Capture> captures1 = match.Groups.Cast<Group>()
                .SelectMany(static g => g.Captures)
                .OfType<Group>()
                .SelectMany(static cc => cc.Captures)
                .ToList();

            List<Capture> captures2 = captures1.OfType<Group>()
                .SelectMany(static g => g.Captures)
                .ToList();

            List<Capture> captures = captures1.Union(captures2)
                .Distinct()
                .ToList();

            string? tagType = captures.FirstOrDefault(
                    static c => c.Value.StartsWith("!", StringComparison.Ordinal) &&
                                (c.Value.IndexOf("#", StringComparison.Ordinal) == -1) &&
                                (c.Value.IndexOf("(", StringComparison.Ordinal) == -1) &&
                                (c.Value.IndexOf(":", StringComparison.Ordinal) == -1)
                )
                ?.Value.TrimStart('!');

            List<string> classes = new();

            if ((tagType?.IndexOf(".", StringComparison.OrdinalIgnoreCase) ?? -1) > -1)
            {
                classes.AddRange(
                    tagType!.Split('.')
                        .Skip(1)
                );
            }

            string? tagId = captures.FirstOrDefault(
                    static c => c.Value.StartsWith("#", StringComparison.Ordinal) &&
                                (c.Value.IndexOf("(", StringComparison.Ordinal) == -1) &&
                                (c.Value.IndexOf(":", StringComparison.Ordinal) == -1)
                )
                ?.Value.TrimStart('#');

            Regex tagAttributeRegex = new Regex(@"^[,]{0,1}\{[^=]+=.*\}", RegexOptions.Compiled);
            List<Capture> tagAttributes = captures.Where(c => tagAttributeRegex.IsMatch(c.Value))
                .ToList();

            List<string> htmlAttributes = tagAttributes.Select(
                    static c => (name: c.Value.TrimStart(",{".ToCharArray())
                        .TrimEnd('}')
                        .Split('=')
                        .First(), value: c.Value.TrimStart(",{".ToCharArray())
                        .TrimEnd('}')
                        .Split('=')
                        .Last())
                )
                .Select(static t => $"{t.name}='{t.value}'")
                .Distinct()
                .ToList();

            string? classAttribute = htmlAttributes.Find(
                static s => s.StartsWith("class=", StringComparison.InvariantCultureIgnoreCase)
            );

            classAttribute ??= "class=''";

            classAttribute = classAttribute[..^1];

            classAttribute += $"{string.Join(" ", classes)}'";

            tagType = tagType?.Split('.')
                .First();

            string? tagValue = captures.FirstOrDefault(
                    static c => c.Value.StartsWith(":", StringComparison.Ordinal)
                )
                ?.Value.TrimStart(':')
                .Trim();

            int counter = 0;
            string formTag = tagType switch
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
                    $"<{tagType} id='{tagId ?? $"{tagType}{counter}"}' {classAttribute} name='{tagId ?? $"{tagType}{counter++}"}' {string.Join(' ', htmlAttributes)} value='{tagValue}'>{tagValue}</{tagType}>",
            };

            sb.Replace(toReplace, formTag);
        }

        return sb.ToString();
    }

    // <([^>]*)md-include="([^"\n]*)"([^>]*)>
    public static async Task<string> ProcessHtmlIncludes(
        string html,
        ConcurrentDictionary<string, object>? variables = null
    )
    {
        StringBuilder sb = new(html);
        Regex regex = new(
            @"<([^>]*)md-include=(""([^""\n]*)""|'([^'\n]*)')([^>]*)>",
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        List<Match> matches = regex.Matches(sb.ToString())
            .ToList();
        List<Match>.Enumerator enumerator = matches.GetEnumerator();

        while (enumerator.MoveNext())
        {
            Match match = enumerator.Current;

            int insertAt = match.Index + match.Length;

            Capture capture = match.Groups.OfType<Capture>()
                .Last(static c => c.Length > 0);

            string? filename = capture.Value.Trim('"');

            if (filename is null or "")
            {
                continue;
            }

            filename = filename.Replace("/", "\\")
                .Trim('\\');

            string root = MarkdownServerOptions.Current!.ServerRoot!;
            filename = Path.Combine(root, filename);

            if (filename is not null or "")
            {
                if (File.Exists(filename))
                {
                    var md = await MarkdownServerOptions.Current.ProcessMarkdown(
                        await File.ReadAllTextAsync(filename),
                        root,
                        variables
                    );

                    string nestedHtml = md.ToHtml(
                        MarkdownServerOptions.Current.Services.GetService<MarkdownPipeline>()
                    );

                    sb.Insert(insertAt, nestedHtml);
                }

                sb.Remove(capture.Index, capture.Length);
            }

            matches = regex.Matches(sb.ToString())
                .ToList();
            enumerator = matches.GetEnumerator();
        }

        enumerator.Dispose();

        return sb.ToString();
    }

    public static async Task<MarkdownDocument> ProcessMarkdown(
        this MarkdownServerOptions options,
        string markdown,
        string myRoot,
        ConcurrentDictionary<string, object>? variables = null
    )
    {
        variables ??= new();

        MarkdownDocument document = MD.Parse(markdown, MarkdownResponse.Pipeline);

        markdown = ProcessFormTags(markdown);
        (var md, var vars) = await MarkdownProcessor.ProcessMarkdownIncludes(markdown);
        markdown = md;

        foreach ((string k, object v) in vars)
        {
            variables.AddOrUpdate(k, v, (_, _) => v);
        }

        MarkdownDocument targetDocument = MD.Parse(markdown, MarkdownResponse.Pipeline);

        for (int i = 0; i < document.Count; ++i)
        {
            Block block = document[i];

            if (i is not 0 ||
                block is not ThematicBreakBlock)
            {
                continue;
            }

            ++i;

            string frontMatter = markdown[document[i]
                .Span.Start..document[i]
                .Span.End];

            Regex tagReplacer = new(@"\t");

            frontMatter = tagReplacer.Replace(frontMatter, "  ")
                .Trim("- ".ToCharArray());

            Deserializer deserializer = new();

            dynamic ddynamicDictionary;

            try
            {
                ddynamicDictionary = deserializer.Deserialize<dynamic>(frontMatter);
            }
            // ReSharper disable once RedundantCatchClause
            catch (Exception)
            {
                throw;
            }


            void RecurseDictionary(string path, Dictionary<object, object> dd)
            {
                foreach (string key in dd.Keys)
                {
                    string newPath = $"{path}.{key}".Trim('.');

                    object value = dd[key];

                    switch (value)
                    {
                        case Dictionary<object, object> d:
                            RecurseDictionary(newPath, d);

                            break;

                        case string s:
                            variables.AddOrUpdate(newPath, s, (_, _) => s);

                            break;

                        case List<object> e:
                        {
                            StringBuilder sb = new();

                            foreach (var subItem in e)
                            {
                                switch (subItem)
                                {
                                    case string ss:
                                        sb.AppendLine($"<p>{ss}</p>");

                                        break;
                                }
                            }

                            variables.AddOrUpdate(newPath, sb.ToString(), (_, _) => sb.ToString());

                            break;
                        }

                        default:
                            variables.AddOrUpdate(
                                newPath,
                                value.ToString() ?? string.Empty,
                                (_, _) => value.ToString() ?? string.Empty
                            );

                            break;
                    }
                }
            }

            RecurseDictionary("", ddynamicDictionary);

            IDictionary<object, object>? dict = ddynamicDictionary as IDictionary<object, object>;

            if (dict?.Keys.FirstOrDefault() is "Variables")
            {
                dict = dict.Values.First() as IDictionary<object, object>;
            }

            await AddDictionary("Variables", dict!);

            async Task AddDictionary(string par, IDictionary<object, object> dictionary)
            {
                // YAML Front Matter
                foreach (string key in dictionary.Keys)
                {
                    string name = $"{par}.{key}";
                    string value = "";

                    var temp = dictionary[key];

                    if (temp is IDictionary<object, object> child)
                    {
                        await AddDictionary(name, child);

                        continue;
                    }

                    if (temp is List<object> list)
                    {
                        value = string.Join('\n', list.Select(static s => $"<p>{s}</p>"));
                    }
                    else
                    {
                        var linestring = dictionary[key]
                                             .ToString() ??
                                         "";

                        value = linestring;
                    }

                    if (name == "Variables.ViewModel")
                    {
                        continue;
                    }

                    if (name.Equals("Variables.Layout", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (File.Exists(value))
                        {
                            value = await File.ReadAllTextAsync(value);
                            document.SetData(name, value);
                        }
                    }

                    variables.AddOrUpdate(name, value, (_, _) => value);
                }
            }

            markdown = markdown.Remove(
                0,
                document[i]
                    .Span.End
            );

            foreach ((string key, object value) in variables)
            {
                switch (value)
                {
                    case string sv when sv.EndsWith(
                        ".md",
                        StringComparison.InvariantCultureIgnoreCase
                    ):
                        sv = Path.GetFullPath(Path.Combine(myRoot, sv));
                        var nestedMarkdown = await options.ProcessNestedMarkdownFile(sv);
                        object text = nestedMarkdown.OfType<LeafBlock>()
                                          .FirstOrDefault()
                                          ?.Inline?.OfType<LiteralInline>()
                                          .FirstOrDefault()
                                          ?.Content.Text ??
                                      value;

                        string nestedHtml = nestedMarkdown.ToHtml(
                            options.Services.GetService<MarkdownPipeline>()
                        );

                        var html = nestedHtml;
                        variables.AddOrUpdate(
                            key,
                            nestedHtml,
                            // ReSharper disable once AccessToModifiedClosure
                            (_, _) => html
                        );

                        if (nestedMarkdown.GetData("Variables") is
                            ConcurrentDictionary<string, object> nestedVariables)
                        {
                            foreach ((string nkey, object nvalue) in nestedVariables)
                            {
                                variables.AddOrUpdate($"{key}_{nkey}", nvalue, (_, _) => nvalue);
                            }
                        }

                        markdown = markdown.Replace($"$({key})", text.ToString());

                        break;

                    case string s when key.All(
                        static c => c is >= 'a' and <= 'z'
                            or >= 'A' and <= 'Z'
                            or >= '0' and <= '9'
                            or '_'
                            or '.'
                    ):
                        markdown = markdown.Replace($"$({key})", s);

                        break;

                    case object o when key.All(
                        static c => c is >= 'a' and <= 'z'
                            or >= 'A' and <= 'Z'
                            or >= '0' and <= '9'
                            or '_'
                            or '.'
                    ):
                    {
                        markdown = markdown.Replace($"$({key})", o.ToString());

                        break;
                    }

                    default:
                        markdown = markdown.Replace($"$({key})", value.ToString());

                        break;
                }
            }

            targetDocument = Markdown.Parse(markdown, MarkdownResponse.Pipeline);
        }

        targetDocument.SetData("Variables", variables);

        return targetDocument;
    }

    public static async Task<IResult> ProcessMarkdownFile(
        this MarkdownServerOptions options,
        string filename,
        ConcurrentDictionary<string, object>? variables = null
    )
        => File.Exists(filename)
            ? (await options.ProcessMarkdown(
                await File.ReadAllTextAsync(filename),
                Path.GetDirectoryName(filename) ?? "",
                variables
            )).ToResult()
            : new MarkdownResponse(HttpStatusCode.NotFound).ToMarkdownResult();

    public static async Task<(string, ConcurrentDictionary<string, object>)>
        ProcessMarkdownIncludes(string markdown)
    {
        StringBuilder sb = new(markdown);
        Regex regex = new(
            @"^#include\(([^)\n]+)\)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        List<Match> matches = regex.Matches(markdown)
            .ToList();
        using List<Match>.Enumerator enumerator = matches.GetEnumerator();

        ConcurrentDictionary<string, object> variables = new();

        if (!enumerator.MoveNext())
        {
            return (sb.ToString(), variables);
        }

        Match match = enumerator.Current;

        int insertAt = match.Index;

        Capture capture = match.Groups.OfType<Capture>()
            .Last(static c => c.Length > 0);

        string? filename = capture.Value.Trim('"');

        if (filename is null or "")
        {
            return (sb.ToString(), variables);
        }

        string? md;

        bool isHttp = filename.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);

        string root = MarkdownServerOptions.Current!.ServerRoot!;

        if (isHttp)
        {
            HttpClient httpClient
                = MarkdownServerOptions.Current.Services.GetRequiredService<HttpClient>();

            md = await httpClient.GetStringAsync(filename);
        }
        else
        {
            filename = Path.Combine(root, filename);

            if (filename is not null or "" &&
                File.Exists(filename))
            {
                md = await File.ReadAllTextAsync(filename);
            }
            else
            {
                return (markdown, variables);
            }
        }

        MarkdownDocument doc = await ProcessMarkdown(MarkdownServerOptions.Current, md, root);

        object? vars = doc.GetData("Variables");

        if (vars is ConcurrentDictionary<string, object> nestedVars)
        {
            foreach ((_, object value) in nestedVars)
            {
                variables.AddOrUpdate($"{capture.Value.Trim('"')}", value, (_, _) => value);
            }
        }

        doc = Markdown.Parse(md, MarkdownProcessor.Pipeline);
        ContainerBlock.Enumerator docEnum = doc.GetEnumerator();

        if (docEnum.MoveNext() &&
            docEnum.Current is ThematicBreakBlock)
        {
            if (docEnum.MoveNext())
            {
                int endIndex = docEnum.Current.Span.End;

                md = md.Remove(0, endIndex)
                    .TrimStart();
            }
        }

        sb.Remove(match.Index, match.Length);
        sb.Insert(insertAt, md);

        var nested = await MarkdownProcessor.ProcessMarkdownIncludes(sb.ToString());

        if (nested.Item1 is not (null or ""))
        {
            sb.Clear();
            sb.Append(nested.Item1);
        }

        if (nested.Item2 is ConcurrentDictionary<string, object> nestedV)
        {
            foreach ((_, object value) in nestedV)
            {
                variables.AddOrUpdate($"{capture.Value.Trim('"')}", value, (_, _) => value);
            }
        }

        docEnum.Dispose();

        return (sb.ToString(), variables);
    }

    public static MarkdownResult ToResult(this MarkdownDocument document)
        => new(document);

    internal static Task<MarkdownDocument> ProcessNestedMarkdownFile(
        this MarkdownServerOptions options,
        string filename
    )
        => File.Exists(filename)
            ? options.ProcessMarkdown(File.ReadAllText(filename), Path.GetDirectoryName(filename)!)
            : options.ProcessMarkdown("Could not located nested file.", Path.GetDirectoryName(filename)!);

    private static MarkdownPipeline? _pipeline;
}
