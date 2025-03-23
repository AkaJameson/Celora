using Microsoft.Extensions.Caching.Distributed;

public interface IRedisRepository : IDistributedCache
{
    Task<long> DecrementAsync(string key, long value = 1);
    byte[]? Get(string key);
    Task<byte[]?> GetAsync(string key, CancellationToken token = default);
    Task<bool> HashDeleteAsync(string key, string field);
    Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);
    Task<T> HashGetAsync<T>(string key, string field);
    Task<bool> HashSetAsync<T>(string key, string field, T value);
    Task<long> IncrementAsync(string key, long value = 1);
    Task<bool> KeyExistsAsync(string key);
    Task<T> ListPopAsync<T>(string key, bool isRight = true);
    Task<long> ListPushAsync<T>(string key, T value, bool isRight = true);
    Task<IEnumerable<T>> ListRangeAsync<T>(string key, long start = 0, long stop = -1);
    Task<long> PublishAsync(string channel, string message);
    void Refresh(string key);
    Task RefreshAsync(string key, CancellationToken token = default);
    void Remove(string key);
    Task RemoveAsync(string key, CancellationToken token = default);
    void Set(string key, byte[] value, DistributedCacheEntryOptions options);
    Task<bool> SetAddAsync<T>(string key, T value);
    Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default);
    Task<bool> SetContainsAsync<T>(string key, T value);
    Task<bool> SetExpiryAsync(string key, TimeSpan expiry);
    Task<IEnumerable<T>> SetMembersAsync<T>(string key);
    Task<long> SetRemoveAsync<T>(string key, IEnumerable<T> values);
    Task SubscribeAsync(string channel, Action<string, string> handler);
}