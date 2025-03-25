namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 分布式事务接口
    /// </summary>
    public interface IShardingTransaction : IDisposable
    {
        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();
        
        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();
    }
} 