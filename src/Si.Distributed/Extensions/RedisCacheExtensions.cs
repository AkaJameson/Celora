using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Si.Distributed.Cache;
using StackExchange.Redis;
using System.Net;

namespace Si.Distributed.Extensions;

/// <summary>
/// Redis缓存扩展方法
/// </summary>
public static class RedisCacheExtensions
{
    private static ConnectionMultiplexer? _connectionMultiplexer;
    private static readonly object _lock = new();

    /// <summary>
    /// 添加Redis分布式缓存
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configurationOptions">Redis配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        ConfigurationOptions configurationOptions)
    {
        // 注册Redis连接
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            if (_connectionMultiplexer == null)
            {
                lock (_lock)
                {
                    _connectionMultiplexer ??= ConnectionMultiplexer.Connect(configurationOptions);
                }
            }
            return _connectionMultiplexer;
        });

        // 注册分布式缓存
        services.AddSingleton<IDistributedCache, RedisDistributedCache>();

        return services;
    }

    /// <summary>
    /// 添加Redis分布式缓存
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">Redis连接字符串</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        string configuration)
    {
        return AddRedisCache(services, ConfigurationOptions.Parse(configuration));
    }

    /// <summary>
    /// 添加Redis分布式缓存（使用多个节点配置）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="endPoints">Redis节点</param>
    /// <param name="password">Redis密码</param>
    /// <param name="defaultDatabase">默认数据库索引</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        EndPointCollection endPoints,
        string? password = null,
        int defaultDatabase = 0)
    {
        var options = new ConfigurationOptions
        {
            Password = password,
            DefaultDatabase = defaultDatabase
        };

        foreach (var endPoint in endPoints)
        {
            options.EndPoints.Add(endPoint);
        }

        return AddRedisCache(services, options);
    }

    /// <summary>
    /// 添加Redis分布式缓存（使用主机和端口）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="host">Redis主机</param>
    /// <param name="port">Redis端口</param>
    /// <param name="password">Redis密码</param>
    /// <param name="defaultDatabase">默认数据库索引</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        string host,
        int port = 6379,
        string? password = null,
        int defaultDatabase = 0)
    {
        var options = new ConfigurationOptions
        {
            EndPoints = { { host, port } },
            Password = password,
            DefaultDatabase = defaultDatabase
        };

        return AddRedisCache(services, options);
    }

    /// <summary>
    /// 添加Redis分布式缓存（使用多个主机和端口）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="endPoints">Redis节点</param>
    /// <param name="password">Redis密码</param>
    /// <param name="defaultDatabase">默认数据库索引</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IEnumerable<(string host, int port)> endPoints,
        string? password = null,
        int defaultDatabase = 0)
    {
        var options = new ConfigurationOptions
        {
            Password = password,
            DefaultDatabase = defaultDatabase
        };

        foreach (var (host, port) in endPoints)
        {
            options.EndPoints.Add(host, port);
        }

        return AddRedisCache(services, options);
    }

    /// <summary>
    /// 获取Redis连接
    /// </summary>
    /// <returns>Redis连接</returns>
    public static ConnectionMultiplexer GetConnection()
    {
        if (_connectionMultiplexer == null)
        {
            throw new InvalidOperationException("Redis连接未初始化，请先调用AddRedisCache方法");
        }
        return _connectionMultiplexer;
    }

    /// <summary>
    /// 获取Redis数据库
    /// </summary>
    /// <param name="db">数据库索引</param>
    /// <returns>Redis数据库</returns>
    public static IDatabase GetDatabase(int db = -1)
    {
        return GetConnection().GetDatabase(db);
    }

    /// <summary>
    /// 获取泛型值
    /// </summary>
    public static T? Get<T>(this IDistributedCache cache, string key) where T : class
    {
        var value = cache.GetString(key);
        if (string.IsNullOrEmpty(value))
            return default;
            
        return System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>
    /// 设置泛型值
    /// </summary>
    public static void Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options) where T : class
    {
        var jsonValue = System.Text.Json.JsonSerializer.Serialize(value);
        cache.SetString(key, jsonValue, options);
    }

    /// <summary>
    /// 设置泛型值（指定过期时间）
    /// </summary>
    public static void Set<T>(this IDistributedCache cache, string key, T value, TimeSpan? absoluteExpiration = null) where T : class
    {
        var options = new DistributedCacheEntryOptions();
        if (absoluteExpiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = absoluteExpiration;
        }

        cache.Set(key, value, options);
    }

    /// <summary>
    /// 异步获取泛型值
    /// </summary>
    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default) where T : class
    {
        var value = await cache.GetStringAsync(key, token);
        if (string.IsNullOrEmpty(value))
            return default;
            
        return System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>
    /// 异步设置泛型值
    /// </summary>
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class
    {
        var jsonValue = System.Text.Json.JsonSerializer.Serialize(value);
        return cache.SetStringAsync(key, jsonValue, options, token);
    }

    /// <summary>
    /// 异步设置泛型值（指定过期时间）
    /// </summary>
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken token = default) where T : class
    {
        var options = new DistributedCacheEntryOptions();
        if (absoluteExpiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = absoluteExpiration;
        }

        return cache.SetAsync(key, value, options, token);
    }
} 