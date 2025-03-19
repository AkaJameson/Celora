using StackExchange.Redis;
namespace Si.Distributed.Lock;


/// <summary>
/// 简单Redis分布式锁实现
/// </summary>
public class RedisLock : IRedisLock
{
    private readonly IConnectionMultiplexer _redis;
    private readonly Dictionary<string, RedisLockObject> _activeLocks;
    private const string LockScript = @"
        if redis.call('exists', KEYS[1]) == 0 then
            redis.call('set', KEYS[1], ARGV[1])
            redis.call('pexpire', KEYS[1], ARGV[2])
            return 1
        end
        return 0";

    private const string UnlockScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        end
        return 0";

    public RedisLock(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _activeLocks = new Dictionary<string, RedisLockObject>();
    }

    /// <summary>
    /// 获取锁（阻塞直到获取成功或超时）
    /// </summary>
    public async Task<IDisposable> AcquireLockAsync(string key, TimeSpan expiry)
    {
        var startTime = DateTime.UtcNow;
        var retryDelay = TimeSpan.FromMilliseconds(100);
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var expiryMilliseconds = (long)expiry.TotalMilliseconds;
        var db = _redis.GetDatabase();

        while (DateTime.UtcNow - startTime < expiry)
        {
            var result = await db.ScriptEvaluateAsync(
                LockScript,
                new RedisKey[] { lockKey },
                new RedisValue[] { lockValue, expiryMilliseconds }
            );

            if ((int)result == 1)
            {
                var lockObject = new RedisLockObject(this, lockKey, lockValue);
                _activeLocks[key] = lockObject;
                return lockObject;
            }

            await Task.Delay(retryDelay);
        }

        throw new TimeoutException($"无法在指定时间内获取锁: {key}");
    }

    /// <summary>
    /// 尝试获取锁（非阻塞）
    /// </summary>
    public async Task<bool> TryAcquireLockAsync(string key, TimeSpan expiry)
    {
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var expiryMilliseconds = (long)expiry.TotalMilliseconds;
        var db = _redis.GetDatabase();

        var result = await db.ScriptEvaluateAsync(
            LockScript,
            new RedisKey[] { lockKey },
            new RedisValue[] { lockValue, expiryMilliseconds }
        );

        if ((int)result == 1)
        {
            var lockObject = new RedisLockObject(this, lockKey, lockValue);
            _activeLocks[key] = lockObject;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 释放锁
    /// </summary>
    public async Task ReleaseLockAsync(string key)
    {
        if (_activeLocks.TryGetValue(key, out var lockObject))
        {
            var db = _redis.GetDatabase();
            await db.ScriptEvaluateAsync(
                UnlockScript,
                new RedisKey[] { lockObject.Key },
                new RedisValue[] { lockObject.Value }
            );
            _activeLocks.Remove(key);
        }
    }

    /// <summary>
    /// 内部锁对象
    /// </summary>
    private class RedisLockObject : IDisposable
    {
        private readonly RedisLock _lock;
        private bool _disposed;

        public string Key { get; }
        public string Value { get; }

        public RedisLockObject(RedisLock redisLock, string key, string value)
        {
            _lock = redisLock;
            Key = key;
            Value = value;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                var plainKey = Key.Substring(5); // 移除"lock:"前缀
                _lock.ReleaseLockAsync(plainKey).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}