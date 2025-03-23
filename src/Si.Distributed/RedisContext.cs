using Si.Distributed.Abstraction;
using StackExchange.Redis;

namespace Si.Distributed
{
    public class RedisContext : IDisposable, IRedisLock, Abstraction.IRedisLockAsync
    {

        private ConnectionMultiplexer connectionMultiplexer;
        public ConnectionMultiplexer ConnectionMultiplexer
        {
            get => connectionMultiplexer;
        }
        /// <summary>
        /// 连接获取方式
        /// </summary>
        public Semaphore Semaphore { get; set; } = new Semaphore(1, 1);
        public RedisContext(Action<ConfigurationOptions> configure)
        {
            var config = new ConfigurationOptions();
            configure?.Invoke(config);
            connectionMultiplexer = ConnectionMultiplexer.Connect(config);
        }

        public IRedisRepository GetRepository()
        {
            Semaphore.WaitOne();
            try
            {
                EnsureConnection();
                return new RedisRepository(ConnectionMultiplexer.GetDatabase());
            }
            catch
            {
                throw;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private void EnsureConnection()
        {
            if (connectionMultiplexer != null && connectionMultiplexer.IsConnected)
            {
                return;
            }
            else
            {
                throw new Exception("Redis has been dispose");
            }
        }
        public void Dispose()
        {
            connectionMultiplexer.Dispose();
        }
        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        public Task<bool> AquireLockAsync(string key, TimeSpan expireSeconds)
        {
            Semaphore.WaitOne();
            EnsureConnection();
            try
            {
                return ConnectionMultiplexer.GetDatabase().LockTakeAsync(key, Guid.NewGuid().ToString(), expireSeconds);
            }
            catch
            {
                throw;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public Task<bool> ReleaseLockAsync(string key)
        {
            Semaphore.WaitOne();
            EnsureConnection();
            try
            {
                return ConnectionMultiplexer.GetDatabase().LockReleaseAsync(key, Guid.NewGuid().ToString());
            }
            catch
            {
                throw;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public bool AquireLock(string key, TimeSpan expireSeconds)
        {
            EnsureConnection();
            Semaphore.WaitOne();
            try
            {
                return ConnectionMultiplexer.GetDatabase().LockTake(key, Guid.NewGuid().ToString(), expireSeconds);
            }
            catch
            {
                throw;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public bool ReleaseLock(string key)
        {
            Semaphore.WaitOne();
            EnsureConnection();
            try
            {
                return ConnectionMultiplexer.GetDatabase().LockRelease(key, Guid.NewGuid().ToString());
            }
            catch
            {
                throw;
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
