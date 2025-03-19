
namespace Si.Distributed.Lock
{
    public interface IRedisLock
    {
        Task<IDisposable> AcquireLockAsync(string key, TimeSpan expiry);
        Task ReleaseLockAsync(string key);
        Task<bool> TryAcquireLockAsync(string key, TimeSpan expiry);
    }
}