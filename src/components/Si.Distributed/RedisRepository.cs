using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

public class RedisRepository : IRedisRepository
{
    private readonly IDatabase _database;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public RedisRepository(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    #region IDistributedCache 基础实现
    public byte[]? Get(string key) => GetAsync(key).GetAwaiter().GetResult();

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return await _database.StringGetAsync(key);
    }

    public void Refresh(string key) => RefreshAsync(key).GetAwaiter().GetResult();

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var ttl = await _database.KeyTimeToLiveAsync(key);
        if (ttl.HasValue) await _database.KeyExpireAsync(key, ttl.Value);
    }

    public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        await _database.KeyDeleteAsync(key);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        => SetAsync(key, value, options).GetAwaiter().GetResult();

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var expiry = GetExpiration(options);
        await _database.StringSetAsync(key, value, expiry);
    }
    #endregion

    #region 扩展方法实现
    public async Task<bool> KeyExistsAsync(string key)
        => await _database.KeyExistsAsync(key);

    public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
        => await _database.KeyExpireAsync(key, expiry);

    // 哈希表操作
    public async Task<bool> HashSetAsync<T>(string key, string field, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return await _database.HashSetAsync(key, field, json);
    }

    public async Task<T> HashGetAsync<T>(string key, string field)
    {
        var json = await _database.HashGetAsync(key, field);
        return json.HasValue ? JsonSerializer.Deserialize<T>(json) : default;
    }

    public async Task<bool> HashDeleteAsync(string key, string field)
        => await _database.HashDeleteAsync(key, field);

    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
    {
        var entries = await _database.HashGetAllAsync(key);
        var result = new Dictionary<string, T>();
        foreach (var entry in entries)
        {
            result.Add(entry.Name, JsonSerializer.Deserialize<T>(entry.Value));
        }
        return result;
    }

    // 列表操作
    public async Task<long> ListPushAsync<T>(string key, T value, bool isRight = true)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return isRight
            ? await _database.ListRightPushAsync(key, json)
            : await _database.ListLeftPushAsync(key, json);
    }

    public async Task<T> ListPopAsync<T>(string key, bool isRight = true)
    {
        var json = isRight
            ? await _database.ListRightPopAsync(key)
            : await _database.ListLeftPopAsync(key);
        return json.HasValue ? JsonSerializer.Deserialize<T>(json) : default;
    }

    public async Task<IEnumerable<T>> ListRangeAsync<T>(string key, long start = 0, long stop = -1)
    {
        var values = await _database.ListRangeAsync(key, start, stop);
        var result = new List<T>();
        foreach (var value in values)
        {
            result.Add(JsonSerializer.Deserialize<T>(value));
        }
        return result;
    }

    // 集合操作
    public async Task<bool> SetAddAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return await _database.SetAddAsync(key, json);
    }

    public async Task<bool> SetContainsAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return await _database.SetContainsAsync(key, json);
    }

    public async Task<long> SetRemoveAsync<T>(string key, IEnumerable<T> values)
    {
        var redisValues = new List<RedisValue>();
        foreach (var value in values)
        {
            redisValues.Add(JsonSerializer.Serialize(value, _jsonOptions));
        }
        return await _database.SetRemoveAsync(key, redisValues.ToArray());
    }

    public async Task<IEnumerable<T>> SetMembersAsync<T>(string key)
    {
        var values = await _database.SetMembersAsync(key);
        var result = new List<T>();
        foreach (var value in values)
        {
            result.Add(JsonSerializer.Deserialize<T>(value));
        }
        return result;
    }

    // 发布订阅
    public async Task<long> PublishAsync(string channel, string message)
        => await _database.PublishAsync(channel, message);

    public async Task SubscribeAsync(string channel, Action<string, string> handler)
    {
        var subscriber = _database.Multiplexer.GetSubscriber();
        await subscriber.SubscribeAsync(channel, (_, message) => handler(channel, message));
    }

    // 原子操作
    public async Task<long> IncrementAsync(string key, long value = 1)
        => await _database.StringIncrementAsync(key, value);

    public async Task<long> DecrementAsync(string key, long value = 1)
        => await _database.StringDecrementAsync(key, value);
    #endregion

    private TimeSpan? GetExpiration(DistributedCacheEntryOptions options)
    {
        return options.AbsoluteExpirationRelativeToNow ??
               options.SlidingExpiration ??
               (options.AbsoluteExpiration.HasValue
                   ? options.AbsoluteExpiration.Value - DateTimeOffset.Now
                   : null);
    }
}