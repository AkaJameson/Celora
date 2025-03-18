using Si.CoreHub.EventBus.Abstraction;

namespace Si.CoreHub.EventBus.Entitys
{
    public class SubscriptionInfo
    {
        public Type EventType { get; }
        public Func<IEvent, Task> Handler { get; }
        public int Priority { get; }
        public Guid Id { get; }
        public SubscriptionInfo(Type eventType, Func<IEvent, Task> handler, int priority)
        {
            EventType = eventType;
            Handler = handler;
            Priority = priority;
            Id = Guid.NewGuid();
        }
    }

}
