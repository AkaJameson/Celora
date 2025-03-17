using MediatR;
using Si.DomainToolkit.Application.CQRS;
using Si.DomainToolkit.Domain.Events;
using Si.DomainToolkit.Examples.OrderManagement.Domain.Aggregates;
using Si.DomainToolkit.Examples.OrderManagement.Domain.ValueObjects;

namespace Si.DomainToolkit.Examples.OrderManagement.Application.Commands
{
    /// <summary>
    /// 创建订单命令处理器
    /// </summary>
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IDomainEventDispatcher _eventDispatcher;

        public CreateOrderCommandHandler(IDomainEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var order = new Order(Guid.NewGuid(), command.OrderNumber, _eventDispatcher);

            foreach (var item in command.Items)
            {
                order.AddItem(
                    item.ProductName,
                    item.Quantity,
                    new Money(item.UnitPrice, item.Currency)
                );
            }

            await order.CompleteAsync(cancellationToken);

            // 在实际应用中，这里需要保存订单到数据库
            // await _orderRepository.SaveAsync(order);

            return order.Id;
        }
    }
} 