using Microsoft.Extensions.DependencyInjection;

namespace Si.CoreHub.MemoryCache
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// 注册缓存服务
        /// </summary>
        /// <param name="services"></param>
        public static void UseMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton(typeof(ICacheService), typeof(MemoryCacheService));
        }
    }
}
