namespace MDS.AspnetServices.Abstractions;

/// <summary>
/// TDD Stub - Processes custom tags/macros from Markdown (e.g. !component, !form).
/// Created before modifying existing regex-heavy implementation.
/// </summary>
public interface IMarkdownTagProcessor
{
    string ProcessTags(string markdown, object? context = null);
    bool TryProcessTag(string tag, out string result);
}