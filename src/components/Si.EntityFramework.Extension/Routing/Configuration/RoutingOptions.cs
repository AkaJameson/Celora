using Microsoft.EntityFrameworkCore;

namespace Si.EntityFramework.Extension.Routing.Configuration
{
    /// <summary>
    /// 数据库路由配置选项
    /// </summary>
    public class RoutingOptions
    {
        /// <summary>
        /// 从库负载均衡策略
        /// </summary>
        public SlaveLoadBalanceStrategy LoadBalanceStrategy { get; set; } = SlaveLoadBalanceStrategy.Random;

        /// <summary>
        /// 从库连接字符串配置
        /// </summary>
        public List<SlaveConnectionConfig> SlaveConnections { get; set; } = new List<SlaveConnectionConfig>();

        /// <summary>
        /// 一致性Hash虚拟节点数 (仅在使用Hashing策略时有效)
        /// </summary>
        public int VirtualNodeCount { get; set; } = 160;

        /// <summary>
        /// 获取一致性Hash的键值（默认使用线程ID）
        /// </summary>
        public Func<string> GetHashKey { get; set; } = () =>
            Thread.CurrentThread.ManagedThreadId.ToString();

    }
    /// <summary>
    /// 从库连接配置
    /// </summary>
    public class SlaveConnectionConfig
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public Action<DbContextOptionsBuilder> optionsBuilder { get; set; }

        /// <summary>
        /// 权重 (用于权重策略，默认为1)
        /// </summary>
        public int Weight { get; set; } = 1;

        /// <summary>
        /// 从库标识 (用于一致性Hash策略)
        /// </summary>
        public string Key { get; set; }
    }
}
