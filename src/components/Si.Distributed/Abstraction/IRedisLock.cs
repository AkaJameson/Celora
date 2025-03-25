namespace Si.Distributed.Abstraction
{
    public interface IRedisLock
    {
        public bool AquireLock(string key, TimeSpan expireSeconds);

        public bool ReleaseLock(string key);
    }
}
