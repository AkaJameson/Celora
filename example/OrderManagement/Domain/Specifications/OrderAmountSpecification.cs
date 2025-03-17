using Si.DomainToolkit.Domain.Specifications;
using Si.DomainToolkit.Examples.OrderManagement.Domain.Aggregates;
using System.Linq.Expressions;

namespace Si.DomainToolkit.Examples.OrderManagement.Domain.Specifications
{
    /// <summary>
    /// 订单金额规范
    /// </summary>
    public class OrderAmountSpecification : Specification<Order>
    {
        private readonly decimal _minAmount;
        private readonly string _currency;

        public OrderAmountSpecification(decimal minAmount, string currency)
        {
            _minAmount = minAmount;
            _currency = currency;
        }

        public override Expression<Func<Order, bool>> ToExpression()
        {
            return order => order.GetTotalAmount().Amount >= _minAmount 
                        && order.GetTotalAmount().Currency == _currency;
        }
    }
} 