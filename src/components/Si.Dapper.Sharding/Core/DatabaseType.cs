namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// SQLite数据库
        /// </summary>
        SQLite = 0,
        
        /// <summary>
        /// MySQL数据库
        /// </summary>
        MySQL = 1,
        
        /// <summary>
        /// PostgreSQL数据库
        /// </summary>
        PostgreSQL = 2,
        
        /// <summary>
        /// SQL Server数据库
        /// </summary>
        SQLServer = 3
    }
} 