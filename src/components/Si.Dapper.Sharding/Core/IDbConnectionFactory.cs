namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 数据库连接工厂接口
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns>数据库连接</returns>
        IDbConnection CreateConnection(string connectionString, DatabaseType dbType);
        
        /// <summary>
        /// 根据配置创建数据库连接
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>数据库连接</returns>
        IDbConnection CreateConnectionFromConfig(string name);
    }
} 