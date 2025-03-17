using MediatR;

namespace Si.DomainToolkit.Examples.OrderManagement.Application.Commands
{
    /// <summary>
    /// 创建订单命令
    /// </summary>
    public class CreateOrderCommand : IRequest<Guid>
    {
        public string OrderNumber { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; }
    }
} 