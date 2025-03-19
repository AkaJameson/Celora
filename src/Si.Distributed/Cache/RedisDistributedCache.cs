using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text;

namespace Si.Distributed.Cache;

/// <summary>
/// Redis分布式缓存实现
/// </summary>
public class RedisDistributedCache : IDistributedCache
{
    private readonly IDatabase _database;

    /// <summary>
    /// 初始化分布式缓存
    /// </summary>
    /// <param name="connectionMultiplexer">Redis连接复用器</param>
    /// <param name="database">数据库索引</param>
    public RedisDistributedCache(IConnectionMultiplexer connectionMultiplexer, int database = 0)
    {
        _database = connectionMultiplexer.GetDatabase(database);
    }

    /// <summary>
    /// 获取字节数组值
    /// </summary>
    public byte[]? Get(string key)
    {
        var value = _database.StringGet(key);
        return value.HasValue ? (byte[])value : null;
    }

    /// <summary>
    /// 异步获取字节数组值
    /// </summary>
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? (byte[])value : null;
    }

    /// <summary>
    /// 刷新缓存项
    /// </summary>
    public void Refresh(string key)
    {
        var ttl = _database.KeyTimeToLive(key);
        if (ttl.HasValue)
        {
            _database.KeyExpire(key, ttl);
        }
    }

    /// <summary>
    /// 异步刷新缓存项
    /// </summary>
    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        var ttl = await _database.KeyTimeToLiveAsync(key);
        if (ttl.HasValue)
        {
            await _database.KeyExpireAsync(key, ttl);
        }
    }

    /// <summary>
    /// 移除缓存项
    /// </summary>
    public void Remove(string key)
    {
        _database.KeyDelete(key);
    }

    /// <summary>
    /// 异步移除缓存项
    /// </summary>
    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return _database.KeyDeleteAsync(key);
    }

    /// <summary>
    /// 设置缓存项
    /// </summary>
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        var expiry = GetExpiry(options);
        _database.StringSet(key, value, expiry);
    }

    /// <summary>
    /// 异步设置缓存项
    /// </summary>
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        var expiry = GetExpiry(options);
        return _database.StringSetAsync(key, value, expiry);
    }

    /// <summary>
    /// 获取字符串值
    /// </summary>
    public string? GetString(string key)
    {
        var value = _database.StringGet(key);
        return value.HasValue ? value.ToString() : null;
    }

    /// <summary>
    /// 异步获取字符串值
    /// </summary>
    public async Task<string?> GetStringAsync(string key, CancellationToken token = default)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    /// <summary>
    /// 设置字符串值
    /// </summary>
    public void SetString(string key, string value, DistributedCacheEntryOptions options)
    {
        var expiry = GetExpiry(options);
        _database.StringSet(key, value, expiry);
    }

    /// <summary>
    /// 异步设置字符串值
    /// </summary>
    public Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        var expiry = GetExpiry(options);
        return _database.StringSetAsync(key, value, expiry);
    }

    /// <summary>
    /// 异步获取泛型值
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken token = default) where T : class
    {
        var value = await GetStringAsync(key, token);
        if (string.IsNullOrEmpty(value))
            return default;
            
        return System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>
    /// 异步设置泛型值
    /// </summary>
    public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class
    {
        var jsonValue = System.Text.Json.JsonSerializer.Serialize(value);
        return SetStringAsync(key, jsonValue, options, token);
    }

    /// <summary>
    /// 判断键是否存在
    /// </summary>
    public Task<bool> ExistsAsync(string key, CancellationToken token = default)
    {
        return _database.KeyExistsAsync(key);
    }

    /// <summary>
    /// 获取过期时间
    /// </summary>
    private TimeSpan? GetExpiry(DistributedCacheEntryOptions options)
    {
        if (options.AbsoluteExpirationRelativeToNow.HasValue)
            return options.AbsoluteExpirationRelativeToNow.Value;
            
        if (options.AbsoluteExpiration.HasValue)
        {
            var seconds = (options.AbsoluteExpiration.Value - DateTimeOffset.Now).TotalSeconds;
            return seconds > 0 ? TimeSpan.FromSeconds(seconds) : TimeSpan.Zero;
        }
            
        if (options.SlidingExpiration.HasValue)
            return options.SlidingExpiration.Value;
            
        return null;
    }
} 