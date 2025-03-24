namespace Si.Modules.EventBus.Abstractions
{
    public interface IEventBus
    {
        public void PublishAsync<T>(T @event) where T : EventBase;
        public Task<bool> PublishAsync<T>(T @event, bool waitResult = false) where T : EventBase;
        public Task SubscribeAsync<T>(Func<T, Task<bool>> handler) where T : EventBase;
    }
}
