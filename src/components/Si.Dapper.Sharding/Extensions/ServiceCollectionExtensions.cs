using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Si.Dapper.Sharding.Core;
using Si.Dapper.Sharding.Implementations;
using Si.Dapper.Sharding.Routing;
using System.Collections.Generic;

namespace Si.Dapper.Sharding.Extensions
{
    /// <summary>
    /// 服务集合扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加分库分表服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddDapperSharding(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDbConnectionFactory>(provider => new DbConnectionFactory(configuration));
            
            return services;
        }

        /// <summary>
        /// 添加表管理器
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddTableManager(this IServiceCollection services)
        {
            services.AddSingleton<ITableManager, TableManager>();
            return services;
        }

        /// <summary>
        /// 添加基于哈希的分库分表服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="tableShardCount">每个库中的分表数量</param>
        /// <param name="databaseConfigs">数据库配置</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddHashSharding(
            this IServiceCollection services,
            string[] databaseNames,
            int tableShardCount,
            Dictionary<string, DatabaseConfig> databaseConfigs,
            string tableShardFormat = "{0}_{1}")
        {
            services.AddSingleton<IShardingRouter>(new HashShardingRouter(databaseNames, tableShardCount, tableShardFormat));
            services.AddSingleton(databaseConfigs);
            services.AddScoped<IShardingDbContext, ShardingDbContext>();
            
            return services;
        }

        /// <summary>
        /// 添加基于取模的分库分表服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="tableShardCount">每个库中的分表数量</param>
        /// <param name="databaseConfigs">数据库配置</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddModSharding(
            this IServiceCollection services,
            string[] databaseNames,
            int tableShardCount,
            Dictionary<string, DatabaseConfig> databaseConfigs,
            string tableShardFormat = "{0}_{1}")
        {
            services.AddSingleton<IShardingRouter>(new ModShardingRouter(databaseNames, tableShardCount, tableShardFormat));
            services.AddSingleton(databaseConfigs);
            services.AddScoped<IShardingDbContext, ShardingDbContext>();
            
            return services;
        }
        
        /// <summary>
        /// 添加动态哈希分库分表服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="tableShardCount">每个库中的分表数量</param>
        /// <param name="databaseConfigs">数据库配置</param>
        /// <param name="tableDefinitions">表定义字典</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddDynamicHashSharding(
            this IServiceCollection services,
            string[] databaseNames,
            int tableShardCount,
            Dictionary<string, DatabaseConfig> databaseConfigs,
            Dictionary<string, ITableDefinition> tableDefinitions,
            string tableShardFormat = "{0}_{1}")
        {
            services.AddSingleton(databaseConfigs);
            services.AddSingleton(tableDefinitions);
            services.AddTableManager();
            services.AddSingleton<IShardingRouter>(provider => 
                new DynamicShardingRouter(
                    databaseNames, 
                    tableShardCount, 
                    provider.GetRequiredService<ITableManager>(),
                    tableDefinitions,
                    provider.GetService<Microsoft.Extensions.Logging.ILogger<DynamicShardingRouter>>(),
                    tableShardFormat));
            
            services.AddScoped<IShardingDbContext, ShardingDbContext>();
            
            return services;
        }
        
        /// <summary>
        /// 添加动态取模分库分表服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="tableShardCount">每个库中的分表数量</param>
        /// <param name="databaseConfigs">数据库配置</param>
        /// <param name="tableDefinitions">表定义字典</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddDynamicModSharding(
            this IServiceCollection services,
            string[] databaseNames,
            int tableShardCount,
            Dictionary<string, DatabaseConfig> databaseConfigs,
            Dictionary<string, ITableDefinition> tableDefinitions,
            string tableShardFormat = "{0}_{1}")
        {
            services.AddSingleton(databaseConfigs);
            services.AddSingleton(tableDefinitions);
            services.AddTableManager();
            services.AddSingleton<IShardingRouter>(provider => 
                new DynamicModShardingRouter(
                    databaseNames, 
                    tableShardCount, 
                    provider.GetRequiredService<ITableManager>(),
                    tableDefinitions,
                    provider.GetService<Microsoft.Extensions.Logging.ILogger<DynamicModShardingRouter>>(),
                    tableShardFormat));
            
            services.AddScoped<IShardingDbContext, ShardingDbContext>();
            
            return services;
        }
        
        /// <summary>
        /// 添加日期分库分表服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="databaseConfigs">数据库配置</param>
        /// <param name="tableDefinitions">表定义字典</param>
        /// <param name="shardingPeriod">分片周期</param>
        /// <param name="historyTableCount">要保留的历史表数量</param>
        /// <param name="futureTableCount">要预创建的未来表数量</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddDateSharding(
            this IServiceCollection services,
            string[] databaseNames,
            Dictionary<string, DatabaseConfig> databaseConfigs,
            Dictionary<string, ITableDefinition> tableDefinitions,
            DateShardingPeriod shardingPeriod = DateShardingPeriod.Month,
            int historyTableCount = 3,
            int futureTableCount = 1,
            string tableShardFormat = "{0}_{1}")
        {
            services.AddSingleton(databaseConfigs);
            services.AddSingleton(tableDefinitions);
            services.AddTableManager();
            services.AddSingleton<DateShardingRouter>(provider => 
                new DateShardingRouter(
                    databaseNames, 
                    provider.GetRequiredService<ITableManager>(),
                    tableDefinitions,
                    shardingPeriod,
                    historyTableCount,
                    futureTableCount,
                    provider.GetService<Microsoft.Extensions.Logging.ILogger<DateShardingRouter>>(),
                    tableShardFormat));
            
            services.AddSingleton<IShardingRouter>(provider => provider.GetRequiredService<DateShardingRouter>());
            services.AddScoped<IShardingDbContext, ShardingDbContext>();
            
            return services;
        }
    }
} 