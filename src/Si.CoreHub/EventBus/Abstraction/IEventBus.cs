namespace Si.CoreHub.EventBus.Abstraction
{
    public interface IEventBus
    {
        void Dispose();
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
        IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler, int priority = 0) where TEvent : IEvent;
    }
    public interface IEvent
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
    }
}