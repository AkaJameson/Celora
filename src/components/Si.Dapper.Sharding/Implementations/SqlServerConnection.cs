using System.Data.SqlClient;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// SQL Server数据库连接
    /// </summary>
    public class SqlServerConnection : BaseDbConnection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public SqlServerConnection(string connectionString)
            : base(new SqlConnection(connectionString), DatabaseType.SQLServer, connectionString)
        {
        }
    }
} 