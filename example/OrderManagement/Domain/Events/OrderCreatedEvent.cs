using Si.DomainToolkit.Domain.Events;
using Si.DomainToolkit.Examples.OrderManagement.Domain.ValueObjects;

namespace Si.DomainToolkit.Examples.OrderManagement.Domain.Events
{
    /// <summary>
    /// 订单创建事件
    /// </summary>
    public class OrderCreatedEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public string OrderNumber { get; }
        public Money TotalAmount { get; }

        public OrderCreatedEvent(Guid orderId, string orderNumber, Money totalAmount)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            TotalAmount = totalAmount;
        }
    }
} 