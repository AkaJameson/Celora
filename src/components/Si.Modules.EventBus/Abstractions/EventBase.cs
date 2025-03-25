namespace Si.Modules.EventBus.Abstractions
{
    public class EventBase : IEvent
    {
        public string Id { get; set; } = new Guid().ToString();
        public virtual EventData Data { get; } // 事件数据
    }
}
