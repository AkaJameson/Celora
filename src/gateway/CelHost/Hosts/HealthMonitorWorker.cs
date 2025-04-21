
using CelHost.Proxy;

namespace CelHost.Hosts
{
    public class HealthMonitorWorker : BackgroundService
    {
        private readonly DestinationHealthCheck healthCheck;
        private readonly IConfiguration configuration;

        public HealthMonitorWorker(DestinationHealthCheck healthCheck, IConfiguration configuration)
        {
            this.healthCheck = healthCheck;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = configuration.GetValue<int>("HealthStateHubRoute:Frequency");
            while (!stoppingToken.IsCancellationRequested)
            {
                await healthCheck.CheckAndBroadcastChangesAsync();
                await Task.Delay(TimeSpan.FromSeconds(interval));
            }
        }
    }
}
