using Markdig;

namespace MDS.AspnetServices.Abstractions;

/// <summary>
/// TDD Stub - Interface for Markdown pipeline creation (created before any implementation changes per strict TDD rule).
/// </summary>
public interface IMarkdownPipelineFactory
{
    MarkdownPipeline CreatePipeline(MarkdownServerOptions options);
    MarkdownPipeline GetDefaultPipeline();
    MarkdownPipeline GetPipelineForTheme(string themeName);
}