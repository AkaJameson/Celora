using MediatR;
using Si.DomainToolkit.Examples.OrderManagement.Domain.Events;

namespace Si.DomainToolkit.Examples.OrderManagement.Application.EventHandlers
{
    /// <summary>
    /// 订单创建事件处理器
    /// </summary>
    public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "订单已创建 - OrderId: {OrderId}, OrderNumber: {OrderNumber}, TotalAmount: {TotalAmount}",
                notification.OrderId,
                notification.OrderNumber,
                notification.TotalAmount);

            // 这里可以添加其他业务逻辑，比如：
            // 1. 发送订单确认邮件
            // 2. 更新库存
            // 3. 生成发票
            // 等等...

            return Task.CompletedTask;
        }
    }
} 