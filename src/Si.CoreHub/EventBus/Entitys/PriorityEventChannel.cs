using Si.CoreHub.EventBus.Abstraction;
using System.Threading.Channels;

namespace Si.CoreHub.EventBus.Entitys
{
    public sealed class PriorityEventChannel<TEvent> where TEvent : IEvent
    {
        private readonly Channel<TEvent> _highPriorityChannel;
        private readonly Channel<TEvent> _normalPriorityChannel;
        private readonly Channel<TEvent> _lowPriorityChannel;

        public PriorityEventChannel()
        {
            var options = new BoundedChannelOptions(10_000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false
            };
            _highPriorityChannel = Channel.CreateBounded<TEvent>(options);
            _normalPriorityChannel = Channel.CreateBounded<TEvent>(options);
            _lowPriorityChannel = Channel.CreateBounded<TEvent>(options);
        }

        public ValueTask WriteAsync(TEvent @event, int priority)
        {
            return priority switch
            {
                >= 100 => _highPriorityChannel.Writer.WriteAsync(@event),
                >= 50 => _normalPriorityChannel.Writer.WriteAsync(@event),
                _ => _lowPriorityChannel.Writer.WriteAsync(@event)
            };
        }

        public IAsyncEnumerable<TEvent> ReadAllAsync(CancellationToken cancellationToken)
        {
            // 按优先级顺序合并多个 Channel 的读取流
            return Merge(
                _highPriorityChannel.Reader.ReadAllAsync(cancellationToken),
                _normalPriorityChannel.Reader.ReadAllAsync(cancellationToken),
                _lowPriorityChannel.Reader.ReadAllAsync(cancellationToken)
            );
        }

        private static async IAsyncEnumerable<TEvent> Merge(params IAsyncEnumerable<TEvent>[] streams)
        {
            var enumerators = streams.Select(s => s.GetAsyncEnumerator()).ToList();
            try
            {
                while (enumerators.Count > 0)
                {
                    for (int i = 0; i < enumerators.Count; i++)
                    {
                        if (await enumerators[i].MoveNextAsync())
                        {
                            yield return enumerators[i].Current;
                        }
                        else
                        {
                            enumerators.RemoveAt(i--);
                        }
                    }
                }
            }
            finally
            {
                foreach (var e in enumerators) await e.DisposeAsync();
            }
        }
    }
}
