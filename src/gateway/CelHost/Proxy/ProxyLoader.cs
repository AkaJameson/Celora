using CelHost.Database;
using CelHost.Proxy.Abstraction;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;

namespace CelHost.Proxy
{
    public class ProxyLoader : IProxyLoader
    {

        private readonly HostContext hostContext;
        public ProxyLoader(InMemoryConfigProvider configProvider,
            HostContext hostContext
           )
        {
            this.hostContext = hostContext;
        }

        public (List<RouteConfig> routes, List<ClusterConfig> clusters) LoadProxyConfigFromDb()
        {
            var clusters = hostContext.Clusters
                .Include(c => c.Nodes)
                .Include(c => c.CheckOption)
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
                    },
                    RateLimiterPolicy = cluster.RateLimitPolicyName
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
                    HealthCheck = new HealthCheckConfig
                    {
                        Active = new ActiveHealthCheckConfig
                        {
                            Enabled = cluster.HealthCheck,
                            Interval = TimeSpan.FromSeconds(cluster.CheckOption.ActiveInterval),
                            Timeout = TimeSpan.FromSeconds(cluster.CheckOption.ActiveTimeout),
                            Policy = HealthCheckConstants.ActivePolicy.ConsecutiveFailures,
                            Path = cluster.CheckOption.ActivePath
                        }
                    }
                };
                clusterConfigs.Add(clusterConfig);
            }

            return (routes, clusterConfigs);
        }

    }
}
