using global::Si.EntityFramework.Extension.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using System;
using System.Data.Common;

namespace Si.EntityFramework.Extension.Routing.Implementations
{
    /// <summary>
    /// 数据库上下文路由实现类
    /// </summary>
    /// <typeparam name="TContext">数据库上下文类型</typeparam>
    public class DbContextRouter<TContext> : IDbContextRouter<TContext> where TContext : ApplicationDbContext
    {
        private readonly RoutingOptions _options;
        private readonly ILoadBalanceSelector _loadBalanceSelector;
        private TContext context;
        private readonly IServiceProvider serviceProvider;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        /// <param name="options">路由配置选项</param>
        public DbContextRouter(IServiceProvider serviceProvider, RoutingOptionsProvider routingOptionsProvider, TContext context)
        {
            var options = routingOptionsProvider[typeof(TContext).Name];
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options;
            // 创建负载均衡选择器
            var factory = new LoadBalanceSelectorFactory(_options);
            _loadBalanceSelector = factory.CreateSelector();
            this.context = context;
            this.serviceProvider = serviceProvider;
        }
        public TContext GetContext() => context;

        public void ForceSlave()
        {
            var slaveConfig = _loadBalanceSelector.Select();
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            slaveConfig.optionsBuilder?.Invoke(optionsBuilder);
            context = ActivatorUtilities.CreateInstance<TContext>(
            serviceProvider,
            optionsBuilder.Options,
            serviceProvider.GetRequiredService<RoutingOptionsProvider>());
        }

        public IUnitOfWork<TContext> GetUnitOfWork()
        {
            return ActivatorUtilities.CreateInstance<IUnitOfWork<TContext>>(serviceProvider, context, serviceProvider);
        }
    }
}
