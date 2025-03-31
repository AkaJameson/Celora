using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace CelHost.Proxy.DynamicProvider
{
    public class Config : IProxyConfig
    {
        public Config(List<RouteConfig> routes, List<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(new CancellationTokenSource().Token);
        }

        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        public IChangeToken ChangeToken { get; }
    }
}
