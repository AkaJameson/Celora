using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Si.Distributed.Abstraction
{
    public interface IRedisLockAsync
    {
        public Task<bool> AquireLockAsync(string key, TimeSpan expireSeconds);

        public Task<bool> ReleaseLockAsync(string key);
    }
}
