using Si.Modules.EventBus.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Si.Modules.EventBus
{
    public class EventBus : IEventBus
    {
        private readonly Channel<IEvent> _events;
        private ConcurrentDictionary<Type, List<Func<IEvent, Task<bool>>>> _subscribers;
        private CancellationTokenSource _cts;
        private ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _callback;

        public Task PublishAsync(IEvent @event, bool waitResult = false)
        {
            throw new NotImplementedException();
        }

        public Task SubscribeAsync<T>(Func<T, Task<bool>> handler) where T : IEvent
        {
            throw new NotImplementedException();
        }
        public async Task ProcessEvent(CancellationToken cancellationToken)
        {
            try
            {
                while(await _events.Reader.WaitToReadAsync(cancellationToken))
                {
                    if(_events.Reader.TryRead(out var @event))
                    {
                        var eventType = @event.GetType();
                        if (_subscribers.TryGetValue(eventType, out var handlers))
                        {
                            var tasks = new List<Task<bool>>();

                        }
                }
            }
        }
    }
}
