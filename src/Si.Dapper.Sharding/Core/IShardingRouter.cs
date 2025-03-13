namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 分片路由接口
    /// </summary>
    public interface IShardingRouter
    {
        /// <summary>
        /// 根据分片键获取数据库名称
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>数据库名称</returns>
        string GetDatabaseName(object shardKey);
        
        /// <summary>
        /// 根据分片键获取表名
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <param name="baseTableName">基础表名</param>
        /// <returns>分片表名</returns>
        string GetTableName(object shardKey, string baseTableName);
        
        /// <summary>
        /// 获取所有数据库名称
        /// </summary>
        /// <returns>数据库名称列表</returns>
        IEnumerable<string> GetAllDatabaseNames();
        
        /// <summary>
        /// 获取指定基础表的所有分片表名
        /// </summary>
        /// <param name="baseTableName">基础表名</param>
        /// <returns>分片表名列表</returns>
        IEnumerable<string> GetAllTableNames(string baseTableName);
    }
} 