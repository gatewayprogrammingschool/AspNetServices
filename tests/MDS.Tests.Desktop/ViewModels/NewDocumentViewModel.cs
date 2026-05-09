using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MDS.Tests.Desktop.Models;

namespace MDS.Tests.Desktop.Dialogs;

public partial class NewDocumentViewModel : ObservableObject
{
    [RelayCommand]
    private Task NewFolder()
    {
        AddingFolder = true;

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void SaveNewFolder(object newFolderName)
    {
        SelectedFolder ??= Folders[0];

        string newFolderPath = Path.Combine(SelectedFolder?.FullPath!, (string)newFolderName);
        Directory.CreateDirectory(newFolderPath);

        Initialize();

        SelectedFolder = Folders.First(fi => fi.FullPath == newFolderPath);

        AddingFolder = false;
    }

    public bool NotAddingFolder => !AddingFolder;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(NotAddingFolder))]
    private bool _addingFolder = false;

    [ObservableProperty]
    private string? _documentName;

    [ObservableProperty]
    private string? _documentPath;

    public IEnumerable<string> DocumentTypeList => Enum.GetNames<DocumentTypes>();

    public string DocumentTypeExtension => DocumentType switch
    {
        DocumentTypes.Data => ".yaml",
        DocumentTypes.StyleSheet => ".css",
        DocumentTypes.Template => ".html",
        DocumentTypes.Resume => ".resume.yml",
        _ => ".md",
    };

    public string DocumentTemplate => DocumentTypeExtension switch
    {
        ".yaml" or ".yml" => Templates.DATA,
        ".css" => Templates.STYLE_SHEET,
        ".html" => Templates.TEMPLATE,
        ".resume.yml" => Templates.RESUME,
        _ => Templates.MARKDOWN,
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedFolderPath))]
    private FolderItem? _selectedFolder;

    public string SelectedFolderPath => $"{(SelectedFolder?.RelativePath ?? "wwwroot").Trim('\\')}\\";

    public string RootPath
    {
        get; private set;
    }

    private DocumentTypes _documentType = DocumentTypes.Markdown;

    public DocumentTypes DocumentType
    {
        get => _documentType;
        set
        {
            if (SetProperty(ref _documentType, value))
            {
                OnPropertyChanged(nameof(DocumentTemplate));
                OnPropertyChanged(nameof(DocumentTypeExtension));
            }
        }
    }

    public string DocumentTypeString
    {
        get => $"{DocumentType}";
        set => DocumentType = Enum.Parse<DocumentTypes>(value);
    }

    [ObservableProperty]
    private ObservableCollection<FolderItem> _folders;

    public NewDocumentViewModel()
    {
    }

    public void Initialize()
    {
        AddingFolder = false;
        DocumentName = "";
        var selected = SelectedFolder;
        SelectedFolder = null;

        RootPath = BlazorProgram.WebApp.Environment.ContentRootPath;

        Folders = new(Directory.GetDirectories(RootPath, "*", SearchOption.AllDirectories)
            .Select(p => new FolderItem($"wwwroot\\{p.Replace(RootPath, "").Trim('\\')}", p))
            .ToList());

        Folders.Insert(0, new("wwwroot", RootPath));

        if (selected is null)
        {
            SelectedFolder = Folders[0];
        }
        else
        {
            SelectedFolder = selected;
        }
    }

    [RelayCommand]
    private async Task CreateDocument(string newDocName)
    {
        var docName = newDocName;
        var ext = Path.GetExtension(newDocName);

        newDocName = ext switch
        {
            ".yaml" or ".yml" or
            ".css" or
            ".html" or ".md" => newDocName,
            _ => $"{newDocName}{DocumentTypeExtension}"
        };

        ext = Path.GetExtension(newDocName);

        DocumentType = ext switch
        {
            ".yaml" or ".yml" => DocumentTypes.Data,
            ".css" => DocumentTypes.StyleSheet,
            ".html" => DocumentTypes.Template,
            ".resume.yml" => DocumentTypes.Resume,
            _ => DocumentTypes.Markdown,
        };

        List<string> sanitized = new();

        foreach (var line in DocumentTemplate.Split('\n'))
        {
            try
            {
                var san = string.Format(line, docName).Trim('\r');

                sanitized.Add(san);
            }
            catch
            {
                sanitized.Add(line);
            }
        }

        var template = string.Join(Environment.NewLine, sanitized);

        var rootPath = RootPath.Remove(RootPath.LastIndexOf("wwwroot")).Trim('\\');

        var filename = Path.Combine(
            rootPath,
            SelectedFolderPath,
            newDocName);

        //Debug.WriteLine($"filename: [{filename}]");

        await File.WriteAllTextAsync(filename, template);

        Reset();

        CloseDialogRequested?.Invoke();
    }

    private const string INNER_REGEX = @"(?<!\|)\{([^\{\}]*)\}(?!\|)";
    private const string OUTER_REGEX = @"(?<!\|)\{((?:\s*\n[^\n\{\}]*(?:\|\{|\}\|)?)*\s*\n)\}(?!\|)";

    private Regex _braceReplacerOuter =
        new(OUTER_REGEX, RegexOptions.Multiline | RegexOptions.Compiled);

    private Regex _braceReplacerInner =
        new(INNER_REGEX, RegexOptions.Multiline | RegexOptions.Compiled);

    private string Sanitize(string documentTemplate)
    {
        while (_braceReplacerInner.IsMatch(documentTemplate))
        {
            documentTemplate = _braceReplacerInner.Replace(documentTemplate, "|{$1}|");
        }

        while (_braceReplacerOuter.IsMatch(documentTemplate))
        {
            documentTemplate = _braceReplacerOuter.Replace(documentTemplate, "|{$1}|");
        }

        documentTemplate = documentTemplate
            .Replace("|{0}|", "{0}")
            .Replace("|{", "{{")
            .Replace("}|", "}}");

        return documentTemplate;
    }

    public void Reset()
    {
        SelectedFolder = null;
        Initialize();
    }

    public event Action CloseDialogRequested;
}

public static class Templates
{
    public const string RESUME = @"---
""$schema"": https://raw.githubusercontent.com/jsonresume/resume-schema/v1.0.0/schema.json
basics:
  name: {0}
  label: Programmer
  image: ''
  email: richard.hendriks@mail.com
  phone: ""(912) 555-4321""
  url: http://richardhendricks.example.com
  summary: Richard hails from Tulsa. He has earned degrees from the University of
    Oklahoma and Stanford. (Go Sooners and Cardinal!) Before starting Pied Piper,
    he worked for Hooli as a part time software developer. While his work focuses
    on applied information theory, mostly optimizing lossless compression schema of
    both the length-limited and adaptive variants, his non-work interests range widely,
    everything from quantum computing to chaos theory. He could tell you about it,
    but THAT would NOT be a “length-limited” conversation!
  location:
    address: 2712 Broadway St
    postalCode: CA 94115
    city: San Francisco
    countryCode: US
    region: California
  profiles:
  - network: Twitter
    username: neutralthoughts
    url: ''
  - network: SoundCloud
    username: dandymusicnl
    url: https://soundcloud.example.com/dandymusicnl
work:
- name: Pied Piper
  location: Palo Alto, CA
  description: Awesome compression company
  position: CEO/President
  url: http://piedpiper.example.com
  startDate: '2013-12-01'
  endDate: '2014-12-01'
  summary: Pied Piper is a multi-platform technology based on a proprietary universal
    compression algorithm that has consistently fielded high Weisman Scores™ that
    are not merely competitive, but approach the theoretical limit of lossless compression.
  highlights:
  - Build an algorithm for artist to detect if their music was violating copy right
    infringement laws
  - Successfully won Techcrunch Disrupt
  - Optimized an algorithm that holds the current world record for Weisman Scores
volunteer:
- organization: CoderDojo
  position: Teacher
  url: http://coderdojo.example.com/
  startDate: '2012-01-01'
  endDate: '2013-01-01'
  summary: Global movement of free coding clubs for young people.
  highlights:
  - Awarded 'Teacher of the Month'
education:
- institution: University of Oklahoma
  url: https://www.ou.edu/
  area: Information Technology
  studyType: Bachelor
  startDate: '2011-06-01'
  endDate: '2014-01-01'
  score: '4.0'
  courses:
  - DB1101 - Basic SQL
  - CS2011 - Java Introduction
awards:
- title: Digital Compression Pioneer Award
  date: '2014-11-01'
  awarder: Techcrunch
  summary: There is no spoon.
publications:
- name: Video compression for 3d media
  publisher: Hooli
  releaseDate: '2014-10-01'
  url: http://en.wikipedia.org/wiki/Silicon_Valley_(TV_series)
  summary: Innovative middle-out compression algorithm that changes the way we store
    data.
skills:
- name: Web Development
  level: Master
  keywords:
  - HTML
  - CSS
  - Javascript
- name: Compression
  level: Master
  keywords:
  - Mpeg
  - MP4
  - GIF
languages:
- language: English
  fluency: Native speaker
interests:
- name: Wildlife
  keywords:
  - Ferrets
  - Unicorns
references:
- name: Erlich Bachman
  reference: It is my pleasure to recommend Richard, his performance working as a
    consultant for Main St. Company proved that he will be a valuable addition to
    any company.
projects:
- name: Miss Direction
  description: A mapping engine that misguides you
  highlights:
  - Won award at AIHacks 2016
  - Built by all women team of newbie programmers
  - Using modern technologies such as GoogleMaps, Chrome Extension and Javascript
  keywords:
  - GoogleMaps
  - Chrome Extension
  - Javascript
  startDate: '2016-08-24'
  endDate: '2016-08-24'
  url: missdirection.example.com
  roles:
  - Team lead
  - Designer
  entity: Smoogle
  type: application
meta:
  canonical: https://raw.githubusercontent.com/jsonresume/resume-schema/master/sample.resume.json
  version: v1.0.0
  lastModified: '2017-12-24T15:53:00'
";

    public const string MARKDOWN = @"
---
Variables:
  title: {0}
  defaultLayout: /wwwroot/DefaultLayout.html
---

# $(Variable.title)

> TODO: Begin typing here.

";

    public const string DATA = @"
Variables:
  Document: {0}
";

    public const string STYLE_SHEET = @"
/* {0} */
/* ---- Front Matter ---- */

/* Pandoc header DIV. Contains .title, .author and .date. Comes before div#TOC.
   Only appears if one of those three are in the document.
*/

body {
    font-family: 'Times New Roman', Times, serif;
    font-size: 12pt;
    line-height: 2;
}

div#header, header {
    /* Put border on bottom. Separates it from TOC or body that comes after it. */
    /*border-bottom: 1px solid #aaa;*/
    margin-bottom: 0.5em;
}

.title /* Pandoc title header (h1.title) */ {
    text-align: center;
}

.author, .date /* Pandoc author(s) and date headers (h2.author and h3.date) */ {
    text-align: center;
}

/* Pandoc table of contents DIV when using the --toc option.
   NOTE: this doesn't support Pandoc's --id-prefix option for #TOC and #header.
   Probably would need to use div[id$='TOC'] and div[id$='header'] as selectors.
*/

div#TOC, nav#TOC {
    /* Put border on bottom to separate it from body. */
    border-bottom: 1px solid #aaa;
    margin-bottom: 0.5em;
}

@media print {
    div#TOC, nav#TOC {
        /* Don't display TOC in print */
        display: none;
    }
}

/* ---- Headers and sections ---- */

h1, h2, h3, h4, h5, h6 {
    page-break-after: avoid;
    text-transform: capitalize;
    font-size: 12pt;
}

h1 {
    text-align: center;
}

h1, h2 {
    font-weight: bold;
    font-style: normal;
}

h3 {
    font-weight: normal;
    font-style: italic;
}

h2, h3, h4, h5, h6 {
    text-align: left;
}

@media print {
    main, section, footer, article {
        page-break-before: always;
    }
}

header.apa td {
    text-align: center
}

section.apa-abstract p {
    text-indent: 0;
}

.apa p {
    text-indent: 0.5in;
}

/* Pandoc with --section-divs option */

/*div div, section section*/ /* Nested sections */ /*{
    margin-left: 2em;*/ /* This will increasingly indent nested header sections */
/*}*/

p {
}

.apa blockquote {
    padding-left: 0.5in;
    font-style: normal;
}

li /* All list items */ {
}

    li > p /* Loosely spaced list item */ {
        /*margin-top: 1em;*/ /* IE: lack of space above a <li> when the item is inside a <p> */
    }

ul /* Whole unordered list */ {
}

    ul li /* Unordered list item */ {
    }

ol /* Whole ordered list */ {
}

    ol li /* Ordered list item */ {
    }

.apa hr {
    width: 100%;
    /*text-align: center;*/
}

/*table tbody tr th,
table tbody tr td {
    border-right: #000 solid 1px;
}

table tbody tr th:nth-last-col,
table tbody tr td:nth-last-col {
    border-right: transparent solid 0;
}*/

/* ---- Some span elements --- */

sub /* Subscripts. Pandoc: H~2~O */ {
}

sup /* Superscripts. Pandoc: The 2^nd^ try. */ {
}

em /* Emphasis. Markdown: *emphasis* or _emphasis_ */ {
}

    em > em /* Emphasis within emphasis: *This is all *emphasized* except that* */ {
        font-style: normal;
    }

strong /* Markdown **strong** or __strong__ */ {
}

/* ---- Links (anchors) ---- */

.apa a /* All links */ {
    /* Keep links clean. On screen, they are colored; in print, they do nothing anyway. */
    text-decoration: none;
}

@media screen {
    a:hover {
        /* On hover, we indicate a bit more that it is a link. */
        text-decoration: underline;
    }
}

@media print {
    a {
        /* In print, a colored link is useless, so un-style it. */
        color: black;
        background: transparent;
    }

        a[href^=""http://""]:after, a[href^=""https://""]:after {
            /* However, links that go somewhere else, might be useful to the reader,
           so for http and https links, print the URL after what was the link
           text in parens
        */
            content: "" ("" attr(href) "") "";
            font-size: 90%;
        }
}

/* ---- Images ---- */

.apa img {
    /* Let it be inline left/right where it wants to be, but verticality make
       it in the middle to look nicer, but opinions differ, and if in a multi-line
       paragraph, it might not be so great.
    */
    vertical-align: middle;
}

.apa div.figure /* Pandoc figure-style image */ {
    /* Center the image and caption */
    margin-left: auto;
    margin-right: auto;
    text-align: center;
    font-style: italic;
}

p.caption /* Pandoc figure-style caption within div.figure */ {
    /* Inherits div.figure props by default */
}

/* ---- Code blocks and spans ---- */

/*pre, code {
    background-color: #fdf7ee;*/
/* BEGIN word wrap */
/* Need all the following to word wrap instead of scroll box */
/* This will override the overflow:auto if present */
/*white-space: pre-wrap;*/ /* css-3 */
/*white-space: -moz-pre-wrap !important;*/ /* Mozilla, since 1999 */
/*white-space: -pre-wrap;*/ /* Opera 4-6 */
/*white-space: -o-pre-wrap;*/ /* Opera 7 */
/*word-wrap: break-word;*/ /* Internet Explorer 5.5+ */
/* END word wrap */
/*}*/

/*pre*/ /* Code blocks */ /*{*/
/* Distinguish pre blocks from other text by more than the font with a background tint. */
/*padding: 0.5em;*/ /* Since we have a background color */
/*border-radius: 5px;*/ /* Softens it */
/* Give it a some definition */
/*border: 1px solid #aaa;*/
/* Set it off left and right, seems to look a bit nicer when we have a background */
/*margin-left: 0.5em;
    margin-right: 0.5em;
}*/

@media screen {
    pre {
        /* On screen, use an auto scroll box for long lines, unless word-wrap is enabled */
        white-space: pre;
        overflow: auto;
        /* Dotted looks better on screen and solid seems to print better. */
        /*border: 1px dotted #777;*/
    }
}

/*code*/ /* All inline code spans */ /*{
}*/

/*p > code, li > code*/ /* Code spans in paragraphs and tight lists */ /*{*/
/* Pad a little from adjacent text */
/*padding-left: 2px;
    padding-right: 2px;
}

li > p code*/ /* Code span in a loose list */ /*{*/
/* We have room for some more background color above and below */
/*padding: 2px;
}*/

/* ---- Math ---- */

span.math /* Pandoc inline math default and --jsmath inline math */ {
    /* Tried font-style:italic here, and it messed up MathJax rendering in some browsers. Maybe don't mess with at all. */
}

div.math /* Pandoc --jsmath display math */ {
}

span.LaTeX /* Pandoc --latexmathml math */ {
}

eq /* Pandoc --gladtex math */ {
}

/* ---- Tables ---- */

/*  A clean textbook-like style with horizontal lines above and below and under
    the header. Rows highlight on hover to help scanning the table on screen.
*/

.apa table {
    border-collapse: collapse;
    border-spacing: 0; /* IE 6 */
    /*border-bottom: 2pt solid #000;*/
    /*border-top: 2pt solid #000;*/ /* The caption on top will not have a bottom-border */
    /* Center */
    margin-left: 0;
    margin-right: 0;
    width: 100%;
}

thead /* Entire table header */ {
    /*border-bottom: 1pt solid #000;*/
    /*background-color: #eee;*/ /* Does this BG print well? */
}

tr.header /* Each header row */ {
}

tbody /* Entire table  body */ {
}

.apa tbody th {
    text-align: center
}

/* Table body rows */

tr {
}

    tr.odd:hover, tr.even:hover /* Use .odd and .even classes to avoid styling rows in other tables */ {
        background-color: #eee;
    }

    /* Odd and even rows */
    tr.odd {
    }

    tr.even {
    }

.apa td,
.apa th /* Table cells and table header cells */ {
    vertical-align: top; /* Word */
    vertical-align: baseline; /* Others */
    /*    padding-left: 0.5em;
    padding-right: 0.5em;
    padding-top: 0.2em;
    padding-bottom: 0.2em;
*/
}

/* Removes padding on left and right of table for a tight look. Good if thead has no background color*/
/*
tr td:last-child, tr th:last-child
    {
    padding-right: 0;
    }
tr td:first-child, tr th:first-child
    {
    padding-left: 0;
    }
*/

.apa th /* Table header cells */ {
    font-weight: bold;
}

.apa tfoot /* Table footer (what appears here if caption is on top?) */ {
}

/*caption*/ /* This is for a table caption tag, not the p.caption Pandoc uses in a div.figure */ /*{
    caption-side: top;
    border: none;
    font-size: 0.9em;
    font-style: italic;
    text-align: center;
    margin-bottom: 0.3em;*/ /* Good for when on top */
/*padding-bottom: 0.2em;
}*/

/* ---- Definition lists ---- */

dl /* The whole list */ {
    /*border-top: 2pt solid black;*/
    /*padding-top: 0.5em;*/
    /*border-bottom: 2pt solid black;*/
}

/*dt*/ /* Definition term */ /*{
    font-weight: bold;
}*/

dd + dt /* 2nd or greater term in the list */ {
    /*    border-top: 1pt solid black;
    padding-top: 0.5em;*/
}

dd /* A definition */ {
    /*margin-bottom: 0.5em;*/
}


/*@media print {
    a[href^=""#fnref""], a.reversefootnote*/ /* Pandoc, MultiMarkdown */ /*{*/
/* Don't display these at all in print since the arrow is only something to click on */
/*display: none;
    }
}*/

/*div.footnotes*/ /* Pandoc footnotes div at end of the document */ /*{
}

    div.footnotes li[id^=""fn""]*/ /* A footnote item within that div */ /*{
    }*/

@media screen {
    body {
        margin-left: 12.5%;
        margin-right: 12.5%;
    }
}

@media print {
    body {
        margin-top: 0.5in;
        margin-left: 0.5in;
        margin-bottom: 0.5in;
        margin-right: 0.5in;
    }

    .noprint {
        display: none;
    }
}


/*
CSS for APA-Style Reference lists,
COPY THE FOLLOWING STYLES INTO YOUR CSS:
*/

/*
Sets any enclosing element (div/ul/ol/dl) with or within the following classes flush left
*/
.apa, .apa ul, .apa ol, .apa dl,
.ref-apa, .ref-apa ul, .ref-apa ol, .ref-apa dl,
.apa-ref, .apa-ref ul, .apa-ref ol, .apa-ref dl,
.refapa, .refapa ul, .refapa ol, .refapa dl,
.aparef, .aparef ul, .aparef ol, .aparef dl {
    padding-left: 0;
    margin-left: 0;
}

    /*
Disables bullets or numbers from appearing on references that use list item (li) elements
*/
    .apa li,
    .ref-apa li,
    .refapa li,
    .apa-ref li,
    .aparef li {
        list-style-type: none;
    }

    /*
Creates the hanging indent and the ‘double spacing’ between references.
*/
    .apa p {
        margin-left: 0; /*this controls how much to indent the lines in your reference.  */
        text-indent: 0.5in; /*to start the first line flush to the left, express in negative here whatever distance you placed in the margin-left setting above. */
        /*        margin-top: 1em;
        margin-bottom: 1em;
*/
    }

    .apa li, .apa dd,
    .ref-apa li, .ref-apa dd,
    .refapa li, .refapa dd,
    .apa-ref li, .apa-ref dd,
    .aparef li, .aparef dd,
    .ref-apa p,
    .refapa p,
    .apa-ref p,
    .aparef p {
        margin-left: 0.5in; /*this controls how much to indent the lines in your reference.  */
        text-indent: -0.5in; /*to start the first line flush to the left, express in negative here whatever distance you placed in the margin-left setting above. */
        /*        margin-top: 1em;
        margin-bottom: 1em;
*/
    }


    /*
The following items are OPTIONAL - Please READ:

DEFINITION TERM styling:
This is usually not needed for blogs
or if you’re already styling your <dt> tags elsewhere.

Tip: Style this element by applying what you are using for your <h2> tags
(or whatever style you feel best represents this hierarchy)
elsewhere in your document,
some generic initial settings are provided below:
*/
    .ref-apa dt {
        font-size: 1.5em;
        font-weight: bold;
        margin: .83em 0;
    }
/*    .ref-apa dd{margin-left: 0;}*/
";

    public const string TEMPLATE = @"<!DOCTYPE html><!-- {0} -->
<html>
<head>
    <title>$(Variables.title)</title>
    <meta charset=""UTF-8"">
    <link rel=""stylesheet"" href=""css/markdown.css"" />
    <!-- HEAD -->
</head>
<body>
    <header md-include=""header.md""></header>
    <main>
        <article>
            $(MarkdownBody)
        </article>
    </main>
    <footer md-include=""footer.md""></footer>
</body>
</html>";

}