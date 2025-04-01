using Microsoft.Extensions.Logging;
using Quartz.Logging;
using Si.Modules.EventBus.Abstractions;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Si.Modules.EventBus
{
    public class EventBus : IEventBus, IDisposable
    {
        private readonly Channel<EventBase> _events;
        private ConcurrentDictionary<string, List<Func<EventBase, Task<bool>>>> _subscribers;
        private CancellationTokenSource _cts;
        private ConcurrentDictionary<string, TaskCompletionSource<bool>> _callback;
        private bool disposedValue = false;
        private readonly ILogger<EventBus> _logger;
        public EventBus(ILogger<EventBus> logger)
        {
            _events = Channel.CreateBounded<EventBase>(1024);
            _subscribers = new ConcurrentDictionary<string, List<Func<EventBase, Task<bool>>>>();
            _cts = new CancellationTokenSource();
            _callback = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();
            _logger = logger;
        }
        public void Start()
        {
            Task.Run(async () =>
            {
                await ProcessEvent(_cts.Token);
            });
        }
        public void PublishAsync<T>(T @event) where T : EventBase
        {
            _events.Writer.TryWrite(@event);
        }

        public async Task<bool> PublishAsync<T>(T @event, bool waitResult = false) where T : EventBase
        {
            var tcs = new TaskCompletionSource<bool>();
            _callback[@event.Id] = tcs;

            await _events.Writer.WriteAsync(@event, _cts.Token); // 写入事件队列

            var result = await tcs.Task;
            _callback.TryRemove(@event.Id, out _);
            return result;
        }

        public Task SubscribeAsync<T>(Func<T, Task<bool>> handler) where T : EventBase
        {
            var eventType = typeof(T);
            var wrapper = new Func<IEvent, Task<bool>>(e => handler((T)e));
            _subscribers.GetOrAdd(typeof(T).Name, new List<Func<EventBase, Task<bool>>>()).Add(wrapper);
            return Task.CompletedTask;
        }
        public async Task ProcessEvent(CancellationToken cancellationToken)
        {
            try
            {
                while (await _events.Reader.WaitToReadAsync(cancellationToken))
                {
                    if (_events.Reader.TryRead(out var @event))
                    {
                        var eventType = @event.GetType();
                        _subscribers.TryGetValue(eventType.Name, out var handlers);
                        if (handlers == null)
                            continue;
                        var tasks = new List<Task<bool>>();
                        foreach (var handler in handlers)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    return await handler(@event);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, ex.Message);
                                    return false;
                                }
                            }, cancellationToken));
                        }
                        var results = await Task.WhenAll(tasks);
                        bool allSuccess = Array.TrueForAll(results, success => success);
                        _callback.TryRemove(@event.Id, out var tcs);
                        if (tcs != null)
                        {
                            tcs.TrySetResult(allSuccess);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cts.Cancel();
                    _callback.Clear();
                }
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


    }
}
