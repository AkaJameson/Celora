using Si.DomainToolkit.Application.CQRS;
using Si.DomainToolkit.Examples.OrderManagement.Domain.Aggregates;

namespace Si.DomainToolkit.Examples.OrderManagement.Application.Queries
{
    /// <summary>
    /// 获取订单查询
    /// </summary>
    public class GetOrderQuery : IQuery<Order>
    {
        public Guid OrderId { get; set; }

        public GetOrderQuery(Guid orderId)
        {
            OrderId = orderId;
        }
    }
} 