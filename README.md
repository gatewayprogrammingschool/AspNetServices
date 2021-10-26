# MarkdownServer for ASP.net.

What it is: _Markdown as Markup Application Server_ for ASP.Net.  

Using it is as simple as importing the Nuget package and adding `builder.AddMarkdownServer()` to `ConfigureServices` and `app.UseMarkdownServer()` in `Configure`.  Or just place this in Program.cs for C# 10:

```csharp
builder.AddMarkdownServer();

//....

app.UserMarkdownServer();
```

## Current Features:

* Serve Markdown file URLs which are rendered through a default `layout.html` or specify the layout in the YAML front matter.
* Include Markdown files using the `#include()` tag in Markdown, or by adding `MDS-Inclue-""` attribute to a block tag in the layout HTML file.
* Create forms and form elements with a simple syntax in Markdown.
* Front-Matter variables can be displayed with `#(variable)` in the Markdown or layout HTML.
* Link directly to Markdown documents, which also be rendered in the HTML layout.

### Example

```markdown
---
Title: Page Title
DefaultValue: This is default.
Layout: Shared/layout.html
---

<!-- from front matter -->
# $(Title) 

<!-- simple POST form -->
!form#myForm({action=result.md},{method=post},{class="form-group"})
!label({for=txtBox}):Search
<!-- user variables in forms -->
!input#txtBox({class="form-element"}):$(DefaultValue)
!button({type=submit}):Go!
!/form
```

## Planned Features before release:

* Code-behind for C# to handle form posts.
* Object model and opinionated application design patterns.

Join the discussion to share your thoughts.
