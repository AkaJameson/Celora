using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Si.AspNetCore.Consul.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsul(this IServiceCollection services, Action<ConsulOptions> configure)
    {
        services.Configure(configure);
        
        services.AddSingleton<IConsulClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ConsulOptions>>().Value;
            return new ConsulClient(config =>
            {
                config.Address = new Uri(options.Address);
            });
        });

        services.AddHostedService<ConsulServiceRegistration>();

        return services;
    }

    public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConsulOptions>(configuration.GetSection("Consul"));
        
        services.AddSingleton<IConsulClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ConsulOptions>>().Value;
            return new ConsulClient(config =>
            {
                config.Address = new Uri(options.Address);
            });
        });

        services.AddHostedService<ConsulServiceRegistration>();

        return services;
    }
} 