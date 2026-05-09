namespace MDS.AspnetServices.Abstractions;

/// <summary>
/// TDD Stub - Renders Markdown to HTML.
/// Created before modifying existing implementation.
/// </summary>
public interface IMarkdownRenderer
{
    Task<string> RenderAsync(string markdown, object? context = null);
}