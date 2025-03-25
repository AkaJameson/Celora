using Microsoft.Extensions.DependencyInjection;
using Si.DomainToolkit.Domain.Events;

namespace Si.DomainToolkit.Infrastructure.MediatR
{
    public static class MediatRExtensions
    {
        public static IServiceCollection AddDomainToolkitMediatR(this IServiceCollection services,Action<MediatRServiceConfiguration> configuration)
        {
            services.AddMediatR(configuration);

            // 注册领域事件分发器
            services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

            return services;
        }
    }
} 