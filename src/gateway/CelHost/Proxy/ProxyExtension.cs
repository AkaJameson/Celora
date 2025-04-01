using CelHost.Data;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Configuration;

namespace CelHost.Proxy
{
    public static class ProxyExtension
    {
        public static void InitializeProxy(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HostContext>();
            var clusters = context.Clusters
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
                        Path = cluster.Path,
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
            //更新配置
            scope.ServiceProvider.GetRequiredService<InMemoryConfigProvider>().Update(routes, clusterConfigs);
        }
    }
}
