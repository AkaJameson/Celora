
namespace Si.Distributed.Lock
{
    public interface IRedisRedLock
    {
        Task<IDisposable> AcquireLockAsync(string resource, TimeSpan expiry, TimeSpan wait, TimeSpan retry);
        Task ReleaseLockAsync(string resource);
        Task<bool> TryAcquireLockAsync(string resource, TimeSpan expiry);
    }
}