using CelHost.Server.Proxy;
using Si.Logging;

namespace CelHost.Server.Hosts
{
    public class HealthMonitorWorker : BackgroundService
    {
        private readonly IConfiguration configuration;
        private IServiceScopeFactory scopeFactory;

        public HealthMonitorWorker(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            this.configuration = configuration;
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = configuration.GetValue<int>("HealthStateHubRoute:Frequency");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
                    var healthCheck = scope.ServiceProvider.GetRequiredService<DestinationHealthCheck>();
                    try
                    {
                        await healthCheck.CheckAndBroadcastChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        logService.Error(ex.Message, ex);
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
                    }
                }

            }
        }
    }
}
