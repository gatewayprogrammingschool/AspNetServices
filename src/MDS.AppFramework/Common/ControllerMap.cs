using System.Collections.Concurrent;

namespace MDS.AppFramework.Common
{
    public sealed class ControllerMap
    {
        private ConcurrentDictionary<PathString, Type> _map = new();

        public (PathString path, Type handlerType) MapPathController<TController>(PathString path)
            where TController : AppController
        {
            var type = _map.GetOrAdd(path, typeof(TController));

            return (path, type);
        }

        public Type? GetControllerType(PathString path) 
            => _map.TryGetValue(path, out Type? type) ? type : null;
    }
}