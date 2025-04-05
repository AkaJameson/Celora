using Yarp.ReverseProxy.Configuration;

namespace CelHost.Proxy.Abstraction
{
    public interface IProxyLoader
    {
        (List<RouteConfig> routes, List<ClusterConfig> clusters) LoadProxyConfigFromDb();
    }
}