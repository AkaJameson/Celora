using MySql.Data.MySqlClient;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// MySQL数据库连接
    /// </summary>
    public class MySqlConnection : BaseDbConnection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public MySqlConnection(string connectionString)
            : base(new global::MySql.Data.MySqlClient.MySqlConnection(connectionString), DatabaseType.MySQL, connectionString)
        {
        }
    }
} 