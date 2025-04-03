using CelHost.Data;
using Microsoft.EntityFrameworkCore;
using Si.Utilites;
using Yarp.ReverseProxy.Configuration;

namespace CelHost.Proxy
{
    public class ProxyLoader
    {
        private readonly InMemoryConfigProvider _configProvider;
        private readonly HostContext hostContext;
        public ProxyLoader(InMemoryConfigProvider configProvider, HostContext hostContext)
        {
            _configProvider = configProvider;
            this.hostContext = hostContext;
        }

        public (List<RouteConfig> routes,List<ClusterConfig> clusters) LoadProxyConfigFromDb()
        {
            var clusters = hostContext.Clusters
                .Include(c => c.Nodes)
                .Where(c => c.Nodes.Any(n => n.IsActive))
                .ToList();

            var routes = new List<RouteConfig>();
            var clusterConfigs = new List<ClusterConfig>();

            foreach (var cluster in clusters)
            {
                // 构建RouteConfig
                var route = new RouteConfig
                {
                    RouteId = cluster.RouteId,
                    ClusterId = cluster.RouteId,
                    Match = new RouteMatch
                    {
                        Path = $"{cluster.Path}/{{**remainder}}"
                    },
                    Transforms = new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"PathRemovePrefix",cluster.Path }
                        }
                    }
                };
                routes.Add(route);

                // 构建ClusterConfig
                var destinations = cluster.Nodes
                    .ToDictionary(
                        n => $"node_{n.Id}",
                        n => new DestinationConfig
                        {
                            Address = n.Address,
                        });

                var clusterConfig = new ClusterConfig
                {
                    ClusterId = cluster.RouteId,
                    Destinations = destinations,
                    LoadBalancingPolicy = "PowerOfTwoChoices",
                };
                clusterConfigs.Add(clusterConfig);
            }

            return (routes, clusterConfigs);
        }

    }
}
