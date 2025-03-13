using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;

namespace Si.EntityFramework.Extension.Routing.Implementations.Selector
{
    /// <summary>
    /// 随机策略选择器
    /// </summary>
    public class RandomSelector : ILoadBalanceSelector
    {
        private readonly List<SlaveConnectionConfig> _slaves;
        private readonly Random _random = new Random();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="slaves">从库连接配置列表</param>
        public RandomSelector(List<SlaveConnectionConfig> slaves)
        {
            if (slaves == null || !slaves.Any())
                throw new ArgumentException("从库连接配置不能为空");

            _slaves = slaves;
        }

        /// <summary>
        /// 随机选择一个从库连接
        /// </summary>
        public SlaveConnectionConfig Select()
        {
            return _slaves[_random.Next(0, _slaves.Count)];
        }
    }
}
