using System.Collections.Concurrent;

namespace MDS.AppFramework.Common;

public sealed class ControllerMap
{
    private readonly ConcurrentDictionary<PathString, PathControllerMapItem> _map =
        new();

    public PathControllerMapItem MapPathController<TController>(PathString path, string? method = null)
        where TController : AppController
    {
        PathControllerMapItem toAdd = new(path, typeof(TController), method);
        return _map.GetOrAdd(path, toAdd);
    }

    public PathControllerMapItem GetControllerType(PathString path)
    {
        return _map.TryGetValue(path, out PathControllerMapItem item) ? item : default;
    }
}
