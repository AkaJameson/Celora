using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Si.AspNetCore.Consul.Extension;

public class ConsulServiceRegistration : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly ConsulOptions _options;
    private string? _serviceId;

    public ConsulServiceRegistration(IConsulClient consulClient, IOptions<ConsulOptions> options)
    {
        _consulClient = consulClient;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _serviceId = $"{_options.ServiceName}_{_options.ServiceId}";

        var registration = new AgentServiceRegistration
        {
            ID = _serviceId,
            Name = _options.ServiceName,
            Address = _options.ServiceAddress,
            Port = _options.ServicePort,
            Tags = _options.Tags,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{_options.ServiceAddress}:{_options.ServicePort}{_options.HealthCheckUrl}",
                Interval = TimeSpan.FromSeconds(_options.HealthCheckInterval),
                Timeout = TimeSpan.FromSeconds(_options.HealthCheckTimeout)
            }
        };
        await _consulClient.Agent.ServiceDeregister(registration.ID, cancellationToken);
        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_serviceId))
        {
            await _consulClient.Agent.ServiceDeregister(_serviceId, cancellationToken);
        }
    }
} 