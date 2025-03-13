using Si.EntityFramework.Extension.Routing.Configuration;
using System.Collections.Concurrent;

namespace Si.EntityFramework.Extension.Routing.Implementations
{
    public class RoutingOptionsProvider
    {
        public ConcurrentDictionary<string, RoutingOptions> RoutingOptionsDict = new();
        public RoutingOptions this[string key]
        {
            get
            {
                if (RoutingOptionsDict.TryGetValue(key, out var options))
                {
                    return options;
                }
                return null;
            }
            set
            {
                RoutingOptionsDict.GetOrAdd(key, _ = value);
            }
        }
    }
}
