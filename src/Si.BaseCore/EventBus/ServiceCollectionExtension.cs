using Microsoft.Extensions.DependencyInjection;
using Si.CoreHub.EventBus.Abstraction;
using Si.CoreHub.EventBus.Entitys;

namespace Si.CoreHub.EventBus
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// 注册内存事件总线
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInMemoryEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, InMemoryEventBus>();
            return services;
        }
        /// <summary>
        /// 注册RabbitMQ事件总线
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMqEventBus(this IServiceCollection services, Action<RabbitMqOptions> configureOptions)
        {
            var options = new RabbitMqOptions();
            configureOptions(options);
            services.AddSingleton(options);
            services.AddSingleton<EventBus.Abstraction.IEventBus, RabbitMqEventBus>();
            return services;

        }
    }
}
