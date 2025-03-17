using Si.DomainToolkit.Domain.Common;
using System.Collections.Generic;

namespace Si.DomainToolkit.Examples.OrderManagement.Domain.ValueObjects
{
    /// <summary>
    /// 金额值对象
    /// </summary>
    public class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public static Money operator +(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");
            return new Money(left.Amount + right.Amount, left.Currency);
        }

        public override string ToString()
        {
            return $"{Amount} {Currency}";
        }
    }
} 