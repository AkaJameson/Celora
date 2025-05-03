using CelHost.Data;
using CelHost.Server.Proxy.Abstraction;
using Yarp.ReverseProxy.Configuration;

namespace CelHost.Server.Proxy
{
    public class ProxyManager : IProxyManager
    {
        private readonly InMemoryConfigProvider _configProvider;
        private readonly IProxyLoader proxyLoader;
        public ProxyManager(InMemoryConfigProvider configProvider, IProxyLoader proxyLoader)
        {
            _configProvider = configProvider;
            this.proxyLoader = proxyLoader;
        }

        public void UpdateProxyConfig()
        {
            (List<RouteConfig> routes, List<ClusterConfig> clusters) = proxyLoader.LoadProxyConfigFromDb();
            _configProvider.Update(routes, clusters);
        }
    }
}
