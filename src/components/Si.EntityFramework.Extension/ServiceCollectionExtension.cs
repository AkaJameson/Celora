using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;
using Si.EntityFramework.Extension.Routing.Implementations;

namespace Si.EntityFramework.Extension
{
    public static class ServiceCollectionExtension
    {
        public static RoutingOptionsProvider RoutingOptionsProvider { get; set; }
        /// <summary>
        /// 添加数据库上下文
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <param name="exOptionAction"></param>
        public static void AddDbContext<TContext>(this IServiceCollection services
            , Action<DbContextOptionsBuilder> optionsAction) where TContext : DbContext
        {
            
            services.AddSingleton<DbContextOptionsBuilder<TContext>>(sp =>
            {
                var builder = new DbContextOptionsBuilder<TContext>();
                optionsAction.Invoke(builder);
                return builder;
            });
            
            services.AddDbContext<TContext>(optionsAction);
        }
        public static void AddDbRouter<TContext>(this IServiceCollection services, Action<RoutingOptions> routingOptions) where TContext : DbContext
        {
            var routingOption = new RoutingOptions();
            routingOptions?.Invoke(routingOption);
            RoutingOptionsProvider[typeof(TContext).Name] = routingOption;
            services.AddScoped<IDbContextRouter<TContext>, DbContextRouter<TContext>>();
        }


    }
}
