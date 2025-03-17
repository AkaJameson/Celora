using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Si.DomainToolkit.Examples.OrderManagement.Application.Commands;
using Si.DomainToolkit.Infrastructure.MediatR;

namespace Si.DomainToolkit.Examples.OrderManagement
{
    public class Example
    {
        public static async Task RunAsync()
        {
            // 1. 设置依赖注入
            var services = new ServiceCollection();
            
            // 2. 添加日志
            services.AddLogging();
            
            // 3. 添加 MediatR
            services.AddDomainToolkitMediatR();
            
            // 4. 构建服务提供者
            var serviceProvider = services.BuildServiceProvider();
            
            // 5. 获取 MediatR
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            
            // 6. 创建订单命令
            var createOrderCommand = new CreateOrderCommand 
            {
                OrderNumber = "ORD-2024-001",
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto 
                    {
                        ProductName = "iPhone 15",
                        Quantity = 1,
                        UnitPrice = 6999.00m,
                        Currency = "CNY"
                    },
                    new OrderItemDto 
                    {
                        ProductName = "AirPods Pro",
                        Quantity = 1,
                        UnitPrice = 1999.00m,
                        Currency = "CNY"
                    }
                }
            };
            
            // 7. 发送命令
            var orderId = await mediator.Send(createOrderCommand);
            
            Console.WriteLine($"订单已创建，ID: {orderId}");
        }
    }
} 