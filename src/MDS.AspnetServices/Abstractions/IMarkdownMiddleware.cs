using Microsoft.AspNetCore.Http;

namespace MDS.AspnetServices.Abstractions;

/// <summary>
/// TDD Stub - Clean DI-based middleware interface for Markdown processing pipeline.
/// Replaces the current static-heavy middleware approach.
/// </summary>
public interface IMarkdownMiddleware
{
    Task InvokeAsync(HttpContext context, RequestDelegate next);
    bool CanHandlePath(string path);
}