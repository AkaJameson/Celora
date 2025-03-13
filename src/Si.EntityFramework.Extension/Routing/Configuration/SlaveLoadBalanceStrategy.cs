namespace Si.EntityFramework.Extension.Routing.Configuration
{
    /// <summary>
    /// 从库负载均衡策略
    /// </summary>
    public enum SlaveLoadBalanceStrategy
    {
        /// <summary>
        /// 随机策略
        /// </summary>
        Random,

        /// <summary>
        /// 轮询策略
        /// </summary>
        RoundRobin,

        /// <summary>
        /// 权重策略
        /// </summary>
        Weighted,

        /// <summary>
        /// 一致性Hash
        /// </summary>
        Hashing,
    }
}
