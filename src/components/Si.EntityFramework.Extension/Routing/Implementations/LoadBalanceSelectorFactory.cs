using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;
using Si.EntityFramework.Extension.Routing.Implementations.Selector;

namespace Si.EntityFramework.Extension.Routing.Implementations
{
    /// <summary>
    /// 负载均衡选择器工厂
    /// </summary>
    public class LoadBalanceSelectorFactory
    {
        private readonly RoutingOptions _options;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">路由配置选项</param>
        public LoadBalanceSelectorFactory(RoutingOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// 创建负载均衡选择器
        /// </summary>
        public ILoadBalanceSelector CreateSelector()
        {
            if (_options.SlaveConnections.Count == 0)
            {
                throw new InvalidOperationException("未配置从库连接");
            }

            switch (_options.LoadBalanceStrategy)
            {
                case SlaveLoadBalanceStrategy.Random:
                    return new RandomSelector(_options.SlaveConnections);

                case SlaveLoadBalanceStrategy.RoundRobin:
                    return new RoundRobinSelector(_options.SlaveConnections);

                case SlaveLoadBalanceStrategy.Weighted:
                    return new WeightedSelector(_options.SlaveConnections);

                case SlaveLoadBalanceStrategy.Hashing:
                    return new ConsistentHashSelector(
                        _options.SlaveConnections,
                        _options.VirtualNodeCount,
                        _options.GetHashKey);

                default:
                    return new RandomSelector(_options.SlaveConnections);
            }
        }
    }
}
