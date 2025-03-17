using Si.DomainToolkit.Domain.Common;
using Si.DomainToolkit.Examples.OrderManagement.Domain.ValueObjects;

namespace Si.DomainToolkit.Examples.OrderManagement.Domain.Entities
{
    /// <summary>
    /// 订单项实体
    /// </summary>
    public class OrderItem : BaseEntity<Guid>
    {
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; }

        private OrderItem() { }

        public OrderItem(Guid id, string productName, int quantity, Money unitPrice) : base(id)
        {
            ProductName = productName;
            SetQuantity(quantity);
            UnitPrice = unitPrice;
        }

        public Money GetTotalPrice()
        {
            return new Money(UnitPrice.Amount * Quantity, UnitPrice.Currency);
        }

        public void SetQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");
            Quantity = quantity;
        }
    }
} 