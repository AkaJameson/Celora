using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Si.Distributed
{
    public static class ServiceCollectionExtension
    {
        public static void AddRedisSupport(this IServiceCollection services, Action<ConfigurationOptions> configure)
        {
            services.AddSingleton(new RedisContext(configure));
            services.AddScoped<IDistributedCache>(sp =>
            {
                return sp.GetRequiredService<RedisContext>().GetRepository();
            });
        }
    }
}
