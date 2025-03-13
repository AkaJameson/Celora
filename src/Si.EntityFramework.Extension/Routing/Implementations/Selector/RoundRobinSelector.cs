using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;

namespace Si.EntityFramework.Extension.Routing.Implementations.Selector
{
    /// <summary>
    /// 轮询策略选择器
    /// </summary>
    public class RoundRobinSelector : ILoadBalanceSelector
    {
        private readonly List<SlaveConnectionConfig> _slaves;
        private int _roundRobinCounter;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="slaves">从库连接配置列表</param>
        public RoundRobinSelector(List<SlaveConnectionConfig> slaves)
        {
            if (slaves == null || !slaves.Any())
                throw new ArgumentException("从库连接配置不能为空");

            _slaves = slaves;
        }

        /// <summary>
        /// 轮询选择一个从库连接
        /// </summary>
        public SlaveConnectionConfig Select()
        {
            int index = Interlocked.Increment(ref _roundRobinCounter) % _slaves.Count;
            return _slaves[index];
        }
    }

}
