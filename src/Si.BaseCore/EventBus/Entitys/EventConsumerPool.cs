using Si.CoreHub.EventBus.Abstraction;

namespace Si.CoreHub.EventBus.Entitys
{
    public class EventConsumerPool<TEvent> where TEvent : IEvent
    {
        private readonly PriorityEventChannel<TEvent> _channel;
        private readonly Func<TEvent, Task> _handler;
        private readonly int _maxConsumers;
        private readonly List<Task> _activeConsumers = new();
        private readonly CancellationTokenSource _cts = new();

        public EventConsumerPool(
            PriorityEventChannel<TEvent> channel,
            Func<TEvent, Task> handler,
            int maxConsumers = 4)
        {
            _channel = channel;
            _handler = handler;
            _maxConsumers = maxConsumers;
        }

        public void Start()
        {
            for (int i = 0; i < _maxConsumers; i++)
            {
                _activeConsumers.Add(Task.Run(ConsumeAsync));
            }
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            await Task.WhenAll(_activeConsumers);
        }

        private async Task ConsumeAsync()
        {
            await foreach (var @event in _channel.ReadAllAsync(_cts.Token))
            {
                try
                {
                    await _handler(@event);
                }
                catch (Exception ex)
                {
                    // 记录错误或重试逻辑
                    Console.WriteLine($"Error handling event {@event.Id}: {ex.Message}");
                }
            }
        }
    }
}
