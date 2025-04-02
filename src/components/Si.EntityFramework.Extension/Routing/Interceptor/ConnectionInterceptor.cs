using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;
using Si.EntityFramework.Extension.Routing.Implementations;
using System.Data.Common;

namespace Si.EntityFramework.Extension.Routing.Interceptor
{
    public class ConnectionInterceptor<TContext> : DbConnectionInterceptor
    {
        private readonly ILoadBalanceSelector _loadBalanceSelector;
        private RoutingOptions routingOptions;
        public ConnectionInterceptor(IServiceProvider serviceProvider)
        {
            var routingOptionsProvider = serviceProvider.GetRequiredService<RoutingOptionsProvider>();
            routingOptions = routingOptionsProvider[typeof(TContext).Name];
            if (routingOptions == null)
            {
                throw new ArgumentNullException(nameof(routingOptions));
            }
            // 创建负载均衡选择器
            var factory = new LoadBalanceSelectorFactory(routingOptions);
            _loadBalanceSelector = factory.CreateSelector();
        }

        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            connection.ConnectionString = GetSlaveConnectionString();
            return base.ConnectionOpening(connection, eventData, result);
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            connection.ConnectionString = GetSlaveConnectionString();
            return await base.ConnectionOpeningAsync(
                connection, eventData, result, cancellationToken);
        }
        private string GetSlaveConnectionString() => _loadBalanceSelector.Select().ConnectionString;
    }
}
