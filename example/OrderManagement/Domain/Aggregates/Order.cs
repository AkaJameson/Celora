using Si.DomainToolkit.Domain.Common;
using Si.DomainToolkit.Domain.Events;
using Si.DomainToolkit.Examples.OrderManagement.Domain.Entities;
using Si.DomainToolkit.Examples.OrderManagement.Domain.Events;
using Si.DomainToolkit.Examples.OrderManagement.Domain.ValueObjects;
using System.Collections.ObjectModel;

namespace Si.DomainToolkit.Examples.OrderManagement.Domain.Aggregates
{
    /// <summary>
    /// 订单聚合根
    /// </summary>
    public class Order : BaseEntity<Guid>, IAggregateRoot
    {
        private readonly List<OrderItem> _items = new();
        private readonly IDomainEventDispatcher _eventDispatcher;

        public string OrderNumber { get; private set; }
        public DateTime OrderDate { get; private set; }
        public IReadOnlyCollection<OrderItem> Items => new ReadOnlyCollection<OrderItem>(_items);

        private Order() { }

        public Order(Guid id, string orderNumber, IDomainEventDispatcher eventDispatcher) : base(id)
        {
            OrderNumber = orderNumber;
            OrderDate = DateTime.UtcNow;
            _eventDispatcher = eventDispatcher;
        }

        public void AddItem(string productName, int quantity, Money unitPrice)
        {
            var item = new OrderItem(Guid.NewGuid(), productName, quantity, unitPrice);
            _items.Add(item);
        }

        public Money GetTotalAmount()
        {
            if (!_items.Any())
                return new Money(0, "CNY");

            return _items.Select(x => x.GetTotalPrice())
                        .Aggregate((x, y) => x + y);
        }

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            var totalAmount = GetTotalAmount();
            var orderCreatedEvent = new OrderCreatedEvent(Id, OrderNumber, totalAmount);
            await _eventDispatcher.PublishAsync(orderCreatedEvent, cancellationToken);
        }
    }
} 