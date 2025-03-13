using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Si.CoreHub.EventBus.Abstraction;
using Si.CoreHub.EventBus.Entitys;
using System.Text;
using System.Text.Json;

namespace Si.CoreHub.EventBus
{
    public class RabbitMqEventBus : Abstraction.IEventBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqEventBus> _logger;
        private readonly string _exchangeName = "event_bus_exchange";
        private readonly string _queueName = "event_bus_queue";

        public RabbitMqEventBus(RabbitMqOptions options, ILogger<RabbitMqEventBus> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the exchange and queue
            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(_queueName, _exchangeName, routingKey: "");

            _logger.LogInformation("RabbitMQ Event Bus initialized.");
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            await Task.Run(() =>
            {
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: "",
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation($"Event published: {typeof(TEvent).Name} with ID {@event.Id}");
            });
        }

        public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler, int priority = 0) where TEvent : IEvent
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<TEvent>(message);

                if (@event != null)
                {
                    await handler(@event);
                }
            };

            var consumerTag = _channel.BasicConsume(_queueName, autoAck: true, consumer: consumer);

            _logger.LogInformation($"Subscribed to event: {typeof(TEvent).Name}");

            return new Subscription(() => _channel.BasicCancel(consumerTag));
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler, int priority = 0) where TEvent : IEvent
        {
            return Subscribe<TEvent>(@event =>
            {
                handler(@event);
                return Task.CompletedTask;
            }, priority);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            _logger.LogInformation("RabbitMQ Event Bus disposed.");
        }
    }
}