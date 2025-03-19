using StackExchange.Redis;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

namespace Si.Distributed.Lock;

/// <summary>
/// Redis RedLock实现（基于多个独立的Redis节点）
/// </summary>
public class RedisRedLockProvider : IRedisRedLock
{
    private readonly RedLockFactory _redLockFactory;
    private readonly Dictionary<string, IRedLock> _activeLocks;
    private readonly List<IConnectionMultiplexer> _connectionMultiplexers;

    /// <summary>
    /// 初始化RedLock提供程序
    /// </summary>
    /// <param name="redisConnectionStrings">多个独立的Redis连接字符串</param>
    public RedisRedLockProvider(IEnumerable<string> redisConnectionStrings)
    {
        var connectionStrings = redisConnectionStrings.ToList();
        if (connectionStrings.Count < 3)
        {
            throw new ArgumentException("RedLock需要至少3个独立的Redis节点才能确保高可用性。");
        }

        // 为每个连接字符串创建独立的连接
        _connectionMultiplexers = new List<IConnectionMultiplexer>();
        var redLockMultiplexers = new List<RedLockMultiplexer>();

        foreach (var connectionString in connectionStrings)
        {
            var connection = ConnectionMultiplexer.Connect(connectionString);
            _connectionMultiplexers.Add(connection);
            redLockMultiplexers.Add(new RedLockMultiplexer(connection));
        }
        
        _redLockFactory = RedLockFactory.Create(redLockMultiplexers);
        _activeLocks = new Dictionary<string, IRedLock>();
    }

    /// <summary>
    /// 获取RedLock锁
    /// </summary>
    public async Task<IDisposable> AcquireLockAsync(string resource, TimeSpan expiry, TimeSpan wait, TimeSpan retry)
    {
        var lockObject = await _redLockFactory.CreateLockAsync(resource, expiry, wait, retry);
        
        if (!lockObject.IsAcquired)
            throw new InvalidOperationException($"无法获取锁: {resource}");

        _activeLocks[resource] = lockObject;
        return lockObject;
    }

    /// <summary>
    /// 尝试获取RedLock锁
    /// </summary>
    public async Task<bool> TryAcquireLockAsync(string resource, TimeSpan expiry)
    {
        var lockObject = await _redLockFactory.CreateLockAsync(
            resource, 
            expiry, 
            TimeSpan.Zero, 
            TimeSpan.Zero);
        
        if (lockObject.IsAcquired)
        {
            _activeLocks[resource] = lockObject;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 释放RedLock锁
    /// </summary>
    public async Task ReleaseLockAsync(string resource)
    {
        if (_activeLocks.TryGetValue(resource, out var lockObject))
        {
            await lockObject.DisposeAsync();
            _activeLocks.Remove(resource);
        }
    }

    /// <summary>
    /// 释放所有资源
    /// </summary>
    public void Dispose()
    {
        foreach (var lockObject in _activeLocks.Values)
        {
            lockObject.Dispose();
        }
        _activeLocks.Clear();

        _redLockFactory.Dispose();

        foreach (var connection in _connectionMultiplexers)
        {
            connection.Dispose();
        }
        _connectionMultiplexers.Clear();
    }
} 