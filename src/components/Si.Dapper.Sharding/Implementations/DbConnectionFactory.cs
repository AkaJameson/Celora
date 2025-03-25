using Microsoft.Extensions.Configuration;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// 数据库连接工厂
    /// </summary>
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration">配置</param>
        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns>数据库连接</returns>
        public IDbConnection CreateConnection(string connectionString, DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.SQLite:
                    return new SQLiteConnection(connectionString);
                case DatabaseType.MySQL:
                    return new MySqlConnection(connectionString);
                case DatabaseType.PostgreSQL:
                    return new PostgreSqlConnection(connectionString);
                case DatabaseType.SQLServer:
                    return new SqlServerConnection(connectionString);
                default:
                    throw new ArgumentException($"不支持的数据库类型：{dbType}");
            }
        }

        /// <summary>
        /// 根据配置创建数据库连接
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>数据库连接</returns>
        public IDbConnection CreateConnectionFromConfig(string name)
        {
            var connSection = _configuration.GetSection($"ConnectionStrings:{name}");
            
            if (!connSection.Exists())
            {
                throw new ArgumentException($"配置 '{name}' 不存在");
            }

            var connString = connSection["ConnectionString"];
            var dbTypeString = connSection["DbType"];
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new ArgumentException($"配置 '{name}' 的连接字符串为空");
            }

            if (string.IsNullOrEmpty(dbTypeString) || !Enum.TryParse<DatabaseType>(dbTypeString, out var dbType))
            {
                throw new ArgumentException($"配置 '{name}' 的数据库类型无效：{dbTypeString}");
            }

            return CreateConnection(connString, dbType);
        }
    }
} 