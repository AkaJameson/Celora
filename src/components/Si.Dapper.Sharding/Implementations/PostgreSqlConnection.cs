using Npgsql;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// PostgreSQL数据库连接
    /// </summary>
    public class PostgreSqlConnection : BaseDbConnection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public PostgreSqlConnection(string connectionString)
            : base(new NpgsqlConnection(connectionString), DatabaseType.PostgreSQL, connectionString)
        {
        }
    }
} 