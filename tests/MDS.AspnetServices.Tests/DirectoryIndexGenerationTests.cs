using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MDS.AspnetServices;
using MDS.AspnetServices.Common;
using System.Collections.Concurrent;
using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace MDS.AspnetServices.Tests;

/// <summary>
/// Tests for the generated directory index behavior when a folder has no index.md.
/// This reproduces and will verify the fix for: links in the generated index
/// for subfolders without index.md do not lead to renderable Markdown documents.
/// </summary>
public class DirectoryIndexGenerationTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _subDir;

    public DirectoryIndexGenerationTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "MdsDirIndexTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);

        _subDir = Path.Combine(_tempRoot, "subfolderWithoutIndex");
        Directory.CreateDirectory(_subDir);

        File.WriteAllText(Path.Combine(_subDir, "DocOne.md"), "# Doc One\n\nThis is the **correct and unique** content for DocOne with a [relative link to DocTwo](DocTwo.md).");
        File.WriteAllText(Path.Combine(_subDir, "DocTwo.md"), "# Doc Two\n\nThis is the **correct and unique** content for DocTwo.\n\nIt contains an image reference ![alt](image.png) and a [link back](./DocOne.md).");
    }

    private static IServiceProvider CreateTestServiceProvider()
    {
        // Minimal provider so MarkdownResponse.Pipeline can resolve the pre-built pipeline.
        // In real app this comes from AddMarkdownServer().
        var services = new ServiceCollection();
        services.AddSingleton(new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task GeneratedDirectoryIndex_ForSubfolderWithoutIndexMd_ContainsCorrectRelativeLinks()
    {
        // Arrange - configure options pointing at our temp structure (simulates mds --root)
        var config = new MarkdownServerConfiguration { DefaultPath = "index.md" };
        var sp = CreateTestServiceProvider();
        var options = new MarkdownServerOptions(sp, config)
        {
            ServerRoot = _tempRoot,
            Services = sp
        };

        var context = new DefaultHttpContext();
        context.Request.Path = "/subfolderWithoutIndex/";

        // Act - this is the code path for folders without index.md
        var result = await options.MarkdownFileExecute(context, "/subfolderWithoutIndex/");

        // Assert - we got a MarkdownResult (the generated index)
        var markdownResult = Assert.IsType<MarkdownResult>(result);
        Assert.NotNull(markdownResult.Document);

        // Render to HTML to inspect the actual links the user would click
        var htmlBytes = await global::MDS.AspnetServices.Common.MarkdownResponse.Create(markdownResult.Document).ToHtmlPage();
        var html = System.Text.Encoding.UTF8.GetString(htmlBytes);

        // The generated index must contain links to the sibling .md files using relative names
        // that, when requested from the subfolder URL, resolve and render correctly.
        Assert.Contains("DocOne", html);
        Assert.Contains("DocTwo", html);
        // Relative links should point to the .md files (current generation produces e.g. DocOne.md)
        Assert.Contains("href=\"DocOne.md\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"DocTwo.md\"", html, StringComparison.OrdinalIgnoreCase);

        // Bonus: the index itself should not contain the distinctive body text from the child files
        Assert.DoesNotContain("correct and unique", html);
    }

    [Fact]
    public async Task MarkdownDocuments_LinkedFromGeneratedIndex_InSubfolderWithoutIndex_RenderSuccessfully()
    {
        // Arrange
        var config = new MarkdownServerConfiguration { DefaultPath = "index.md" };
        var sp = CreateTestServiceProvider();
        var options = new MarkdownServerOptions(sp, config)
        {
            ServerRoot = _tempRoot,
            Services = sp
        };

        // Simulate what happens when user clicks a link from the generated index:
        // direct request for /subfolderWithoutIndex/DocOne.md
        var context = new DefaultHttpContext();
        context.Request.Path = "/subfolderWithoutIndex/DocOne.md";

        // Act - normal file processing path (not the directory generation path)
        var result = await options.MarkdownFileExecute(context, "/subfolderWithoutIndex/DocOne.md");

        // Assert - must be a successful rendered result, not NotFound or error
        var markdownResult = Assert.IsType<MarkdownResult>(result);
        Assert.NotNull(markdownResult.Document);

        // The document must contain the *correct* content from DocOne.md (not just "some" content).
        var htmlBytes = await global::MDS.AspnetServices.Common.MarkdownResponse.Create(markdownResult.Document).ToHtmlPage();
        var html = System.Text.Encoding.UTF8.GetString(htmlBytes);

        Assert.Contains("<h1 id=\"doc-one\">Doc One</h1>", html);
        Assert.Contains("This is the <strong>correct and unique</strong> content for DocOne", html);
        Assert.Contains("<a href=\"DocTwo.md\">relative link to DocTwo</a>", html);
        Assert.DoesNotContain("Not Found", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("# Doc One", html); // no raw markdown leakage
    }

    [Fact]
    public async Task MdsTool_FullPipeline_SubfolderWithoutIndexMd_GeneratedIndexLinksRenderTargetDocuments()
    {
        // This test closely mimics what the `mds` CLI tool does:
        // 1. User points mds --root at a content directory containing subfolders without index.md
        // 2. Requesting the subfolder generates an index with links
        // 3. Following those links must return properly rendered HTML (not raw markdown, not 404)

        // Build a host exactly like MDS.Tool/Program.cs does for a given --root
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = _tempRoot,
            WebRootPath = _tempRoot
        });

        builder.WebHost.UseTestServer(); // Enables in-memory server for testing
        builder.AddMarkdownServer();

        var app = builder.Build();
        app.UseDefaultFiles();
        app.UseMarkdownServer();
        app.UseStaticFiles();

        await app.StartAsync();

        var client = app.GetTestClient();

        // Act 1: Request the subfolder without index.md (this triggers generated index)
        var indexResponse = await client.GetAsync("/subfolderWithoutIndex/");
        var indexHtml = await indexResponse.Content.ReadAsStringAsync();

        // Assert on generated index
        Assert.Equal(System.Net.HttpStatusCode.OK, indexResponse.StatusCode);
        Assert.StartsWith("text/html", indexResponse.Content.Headers.ContentType?.ToString());
        Assert.Contains("Directory Index", indexHtml);
        Assert.Contains("DocOne.md", indexHtml);
        Assert.Contains("DocTwo.md", indexHtml);

        // Act 2: Follow one of the links that the generated index would produce
        var linkedDocResponse = await client.GetAsync("/subfolderWithoutIndex/DocTwo.md");
        var linkedHtml = await linkedDocResponse.Content.ReadAsStringAsync();

        // Assert: The linked document must be rendered as HTML (the fix we are proving)
        Assert.Equal(System.Net.HttpStatusCode.OK, linkedDocResponse.StatusCode);
        Assert.StartsWith("text/html", linkedDocResponse.Content.Headers.ContentType?.ToString());

        // Must contain the *correct* rendered content from DocTwo.md (not just "some" content).
        // Verify precise HTML output produced by the Markdown pipeline for this exact source.
        Assert.Contains("<h1 id=\"doc-two\">Doc Two</h1>", linkedHtml);
        Assert.Contains("This is the <strong>correct and unique</strong> content for DocTwo.", linkedHtml);
        Assert.Contains("<a href=\"./DocOne.md\">link back</a>", linkedHtml);  // internal relative link preserved
        Assert.Contains("<img src=\"image.png\" alt=\"alt\" />", linkedHtml); // Markdig auto-renders images to <img> tags
        Assert.DoesNotContain("# Doc Two", linkedHtml); // raw markdown must not leak into final HTML
        Assert.DoesNotContain("correct and unique content for DocOne", linkedHtml); // ensure we rendered DocTwo's content, not DocOne's body

        await app.StopAsync();
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, recursive: true); } catch { /* best effort cleanup */ }
    }
}
