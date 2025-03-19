using Microsoft.Extensions.DependencyInjection;
using Si.Distributed.Lock;

namespace Si.Distributed.Extensions;

/// <summary>
/// RedLock扩展方法
/// </summary>
public static class RedLockExtensions
{
    /// <summary>
    /// 添加RedLock分布式锁服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="connectionStrings">Redis连接字符串集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRedLock(
        this IServiceCollection services,
        params string[] connectionStrings)
    {
        if (connectionStrings.Length < 3)
        {
            throw new ArgumentException("RedLock需要至少3个独立的Redis节点才能确保高可用性。");
        }

        services.AddSingleton<IRedisRedLock>(provider => new RedisRedLockProvider(connectionStrings));
        return services;
    }

    /// <summary>
    /// 添加RedLock分布式锁服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="redisEndpoints">Redis节点配置集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRedLock(
        this IServiceCollection services,
        params (string host, int port, string? password, int database)[] redisEndpoints)
    {
        if (redisEndpoints.Length < 3)
        {
            throw new ArgumentException("RedLock需要至少3个独立的Redis节点才能确保高可用性。");
        }

        var connectionStrings = redisEndpoints
            .Select(endpoint =>
            {
                var connStr = $"{endpoint.host}:{endpoint.port}";

                if (!string.IsNullOrEmpty(endpoint.password))
                    connStr += $",password={endpoint.password}";

                if (endpoint.database != 0)
                    connStr += $",defaultDatabase={endpoint.database}";

                return connStr;
            })
            .ToArray();

        return AddRedLock(services, connectionStrings);
    }
} 