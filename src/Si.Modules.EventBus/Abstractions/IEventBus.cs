using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Si.Modules.EventBus.Abstractions
{
    public interface IEventBus
    {
        public Task PublishAsync(IEvent @event, bool waitResult = false);
        public Task SubscribeAsync<T>(Func<T, Task<bool>> handler) where T : IEvent;
    }
}
