using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using MDS.AppFramework.Common;

using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls
{
    public class Label : IControl
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public bool IsPostBack { get; }
        public IViewState? Parent { get; set; }
        public ILogger? Logger { get; set; }
        public ConcurrentDictionary<string, LazyContainer> ViewState { get; set; }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public Task InitAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task InitializePageStateAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task PreRenderAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task ProcessPageAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task RenderAsync(HttpContext context, HttpResponseStreamWriter writer, HtmlEncoder htmlEncoder)
        {
            throw new NotImplementedException();
        }
    }
}
