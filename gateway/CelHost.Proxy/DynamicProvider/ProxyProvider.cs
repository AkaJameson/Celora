using Yarp.ReverseProxy.Configuration;

namespace CelHost.Proxy.DynamicProvider
{
    public class ProxyProvider : IProxyConfigProvider
    {
        private volatile Config _config;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public IProxyConfig GetConfig() => _config;
        public void UpdateRouteAndCluster(List<RouteConfig> routes, List<ClusterConfig> clusters)
        {
            _semaphore.Wait();
            _config = new Config(routes, clusters);
            _semaphore.Release();
        }
    }
}
