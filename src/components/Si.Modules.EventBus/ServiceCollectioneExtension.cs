using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Si.Modules.EventBus.Abstractions;

namespace Si.Modules.EventBus
{
    public static class ServiceCollectioneExtension
    {
        public static void AddEventBus(this IServiceCollection services)
        {

            services.AddSingleton<IEventBus>(sp =>
            {
                var log = sp.GetRequiredService<ILogger<EventBus>>();
                var eventBus = new EventBus(log);
                eventBus.Start();
                return eventBus;
            });
        }
    }
}
