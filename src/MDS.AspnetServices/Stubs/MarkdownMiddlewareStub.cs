using Microsoft.AspNetCore.Http;
using MDS.AspnetServices.Abstractions;

namespace MDS.AspnetServices.Stubs;

public class MarkdownMiddlewareStub : IMarkdownMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        throw new NotImplementedException("TDD Stub - MarkdownMiddlewareStub");
    }

    public bool CanHandlePath(string path)
    {
        throw new NotImplementedException("TDD Stub - MarkdownMiddlewareStub");
    }
}