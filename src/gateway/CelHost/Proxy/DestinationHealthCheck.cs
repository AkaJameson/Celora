using CelHost.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Model;

namespace CelHost.Proxy
{
    public class DestinationHealthCheck
    {
        private readonly IProxyStateLookup proxyStateLookup;
        private readonly IHubContext<HealthHub> _hubContext;
        public DestinationHealthCheck(IProxyStateLookup proxyStateLookup)
        {
            this.proxyStateLookup = proxyStateLookup;
        }
        public async Task CheckAndBroadcastChangesAsync()
        {
            var DestinationHealthStates = new ConcurrentDictionary<string, List<Dto.DestinationModel>>();
            var clusters = proxyStateLookup.GetClusters();
            if (clusters == null) return;

            foreach (var cluster in clusters)
            {
                var destinations = cluster.Destinations;
                var destinalModelList = new List<Dto.DestinationModel>();
                foreach (var destination in destinations.Values)
                {
                    var stateModel = new Dto.DestinationModel()
                    {
                        DestinationId = destination.DestinationId,
                        HealtState = destination.Health.Active
                    };
                    destinalModelList.Add(stateModel);
                }
                DestinationHealthStates.TryAdd(cluster.ClusterId, destinalModelList);
            }
            await _hubContext.Clients.All.SendAsync("UpdateHealthState", DestinationHealthStates);
        }
    }
}
