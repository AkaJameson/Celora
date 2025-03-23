namespace Si.Modules.EventBus.Abstractions
{
    public interface IEvent
    {
        public string Id { get; set; }
        public TimeSpan DelayTime { get; set; }
    }
}
