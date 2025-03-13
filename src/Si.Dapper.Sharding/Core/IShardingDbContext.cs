using System.Data;

namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 分片数据库上下文接口
    /// </summary>
    public interface IShardingDbContext
    {
        /// <summary>
        /// 获取分片数据库连接
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>数据库连接</returns>
        IDbConnection GetConnection(object shardKey);
        
        /// <summary>
        /// 执行跨分片查询
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns>查询结果</returns>
        IEnumerable<T> QueryAcrossShards<T>(string sql, object? param = null);
        
        /// <summary>
        /// 执行跨分片操作
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns>影响行数</returns>
        int ExecuteAcrossShards(string sql, object? param = null);
        
        /// <summary>
        /// 开始分布式事务
        /// </summary>
        /// <returns>分布式事务</returns>
        IShardingTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
} 