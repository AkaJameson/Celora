using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Data.Configurations;
using Si.EntityFramework.Extension.Data.Context;
using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;
using Si.EntityFramework.Extension.Routing.Implementations;

namespace Si.EntityFramework.Extension
{
    public static class ServiceCollectionExtension
    {
        public static DbOptionsProvider DbOptionsProvider { get; set; }
        public static RoutingOptionsProvider RoutingOptionsProvider { get; set; }
        /// <summary>
        /// 添加数据库上下文
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <param name="exOptionAction"></param>
        public static void AddApplicationDbContext<TContext>(this IServiceCollection services
            , Action<DbContextOptionsBuilder> optionsAction
            , Action<DbOptions> exOptionAction) where TContext : ApplicationDbContext
        {
            if (DbOptionsProvider == null)
            {
                DbOptionsProvider = new DbOptionsProvider();
                services.AddSingleton(DbOptionsProvider);
            }
            services.AddSingleton<DbContextOptionsBuilder<TContext>>(sp =>
            {
                var builder = new DbContextOptionsBuilder<TContext>();
                optionsAction.Invoke(builder);
                return builder;
            });
            var dbOptions = new DbOptions();
            exOptionAction?.Invoke(dbOptions);
            DbOptionsProvider[typeof(TContext).Name] = dbOptions;
            services.AddDbContext<ApplicationDbContext>(optionsAction);
        }
        public static void AddDbRouter<TContext>(this IServiceCollection services, Action<RoutingOptions> routingOptions) where TContext : ApplicationDbContext
        {
            services.AddSingleton(DbOptionsProvider);
            var routingOption = new RoutingOptions();
            routingOptions?.Invoke(routingOption);
            RoutingOptionsProvider[typeof(TContext).Name] = routingOption;
            services.AddScoped<IDbContextRouter<TContext>, DbContextRouter<TContext>>();
        }


    }
}
