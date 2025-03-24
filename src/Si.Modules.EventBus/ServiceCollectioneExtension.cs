using Microsoft.Extensions.DependencyInjection;
using Si.Modules.EventBus.Abstractions;

namespace Si.Modules.EventBus
{
    public static class ServiceCollectioneExtension
    {
        public static void AddEventBus(this IServiceCollection services)
        {

            services.AddSingleton<IEventBus>(sp =>
            {
                var eventBus = new EventBus();
                eventBus.Start();
                return eventBus;
            });
        }
    }
}
