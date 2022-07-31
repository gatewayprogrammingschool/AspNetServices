using System.Collections.Concurrent;

using MDS.AppFramework.Common;
using MDS.AspnetServices.Common;

namespace MDS.AppFramework.Controls;

public interface IViewState
{
    ConcurrentDictionary<string, LazyContainer> ViewState
    {
        get;
        set;
    }

    Task InitializePageStateAsync(HttpContext context);

    internal async Task DeserializePageStateAsync(HttpContext context, Stream stream)
        => ViewState
            = (await stream.DeserializeAsync<ConcurrentDictionary<string, LazyContainer>>()) ??
              new ConcurrentDictionary<string, LazyContainer>();

    internal Task SerializePageStateAsync(HttpContext context, Stream stream)
        => stream.SerializeAsync(ViewState);
}
