using Si.CoreHub.EventBus.Abstraction;
using Si.CoreHub.EventBus.Entitys;
using System.Collections.Concurrent;

namespace Si.CoreHub.EventBus
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, List<SubscriptionInfo>> _handlers = new();
        private readonly PriorityEventChannel<IEvent> _eventChannel;
        private readonly EventConsumerPool<IEvent> _consumerPool;

        public InMemoryEventBus()
        {
            _eventChannel = new PriorityEventChannel<IEvent>();
            _consumerPool = new EventConsumerPool<IEvent>(
                _eventChannel,
                async @event =>
                {
                    if (_handlers.TryGetValue(@event.GetType(), out var subscriptions))
                    {
                        var tasks = subscriptions
                            .OrderByDescending(s => s.Priority)
                            .Select(s => s.Handler(@event));
                        await Task.WhenAll(tasks);
                    }
                },
                maxConsumers: Environment.ProcessorCount * 2
            );
            _consumerPool.Start();
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            // 根据订阅者的平均优先级确定事件优先级
            var priority = _handlers.TryGetValue(typeof(TEvent), out var subs)
                ? subs.Average(s => s.Priority)
                : 50;
            await _eventChannel.WriteAsync(@event, (int)priority);
        }

        public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler, int priority = 0) where TEvent : IEvent
        {
            var subscription = new SubscriptionInfo(
                typeof(TEvent),
                async e => await handler((TEvent)e),
                priority
            );

            _handlers.AddOrUpdate(
                typeof(TEvent),
                new List<SubscriptionInfo> { subscription },
                (_, list) => { list.Add(subscription); return list; }
            );

            return new Subscription(() => Unsubscribe<TEvent>(subscription.Id));
        }

        private void Unsubscribe<TEvent>(Guid subscriptionId)
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var subs))
            {
                subs.RemoveAll(s => s.Id == subscriptionId);
            }
        }

        public void Dispose()
        {
            _consumerPool.StopAsync().Wait();
        }
    }
}