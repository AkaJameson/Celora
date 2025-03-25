using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;

namespace Si.EntityFramework.Extension.Routing.Implementations.Selector
{
    /// <summary>
    /// 权重策略选择器
    /// </summary>
    public class WeightedSelector : ILoadBalanceSelector
    {
        private readonly List<SlaveConnectionConfig> _slaves;
        private readonly List<SlaveConnectionConfig> _weightedSlaves;
        private readonly Random _random = new Random();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="slaves">从库连接配置列表</param>
        public WeightedSelector(List<SlaveConnectionConfig> slaves)
        {
            if (slaves == null || !slaves.Any())
                throw new ArgumentException("从库连接配置不能为空");

            _slaves = slaves;

            // 根据权重扩展列表
            _weightedSlaves = new List<SlaveConnectionConfig>();
            foreach (var slave in _slaves)
            {
                // 确保权重至少为1
                int weight = Math.Max(1, slave.Weight);
                for (int i = 0; i < weight; i++)
                {
                    _weightedSlaves.Add(slave);
                }
            }
        }

        /// <summary>
        /// 根据权重选择一个从库连接
        /// </summary>
        public SlaveConnectionConfig Select()
        {
            return _weightedSlaves[_random.Next(0, _weightedSlaves.Count)];
        }
    }

}
