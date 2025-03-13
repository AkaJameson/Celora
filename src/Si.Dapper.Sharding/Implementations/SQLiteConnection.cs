using Microsoft.Data.Sqlite;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// SQLite数据库连接
    /// </summary>
    public class SQLiteConnection : BaseDbConnection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public SQLiteConnection(string connectionString)
            : base(new SqliteConnection(connectionString), DatabaseType.SQLite, connectionString)
        {
        }
    }
} 