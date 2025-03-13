using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// 分片数据库上下文实现
    /// </summary>
    public class ShardingDbContext : IShardingDbContext
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IShardingRouter _router;
        private readonly Dictionary<string, DatabaseConfig> _databaseConfigs;
        private readonly ILogger<ShardingDbContext> _logger;
        private readonly Dictionary<string, Core.IDbConnection> _connections = new Dictionary<string, Core.IDbConnection>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionFactory">连接工厂</param>
        /// <param name="router">分片路由</param>
        /// <param name="databaseConfigs">数据库配置</param>
        /// <param name="logger">日志记录器</param>
        public ShardingDbContext(
            IDbConnectionFactory connectionFactory,
            IShardingRouter router,
            Dictionary<string, DatabaseConfig> databaseConfigs,
            ILogger<ShardingDbContext> logger = null)
        {
            _connectionFactory = connectionFactory;
            _router = router;
            _databaseConfigs = databaseConfigs;
            _logger = logger;
        }

        /// <summary>
        /// 获取分片数据库连接
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>数据库连接</returns>
        public Core.IDbConnection GetConnection(object shardKey)
        {
            var dbName = _router.GetDatabaseName(shardKey);
            
            if (!_databaseConfigs.TryGetValue(dbName, out var dbConfig))
            {
                throw new ArgumentException($"未找到数据库配置：{dbName}");
            }

            string connectionCacheKey = $"{dbConfig.ConnectionString}_{dbConfig.DbType}";

            if (!_connections.TryGetValue(connectionCacheKey, out var connection))
            {
                connection = _connectionFactory.CreateConnection(dbConfig.ConnectionString, dbConfig.DbType);
                _connections.Add(connectionCacheKey, connection);
            }

            return connection;
        }

        /// <summary>
        /// 执行跨分片查询
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns>查询结果</returns>
        public IEnumerable<T> QueryAcrossShards<T>(string sql, object param = null)
        {
            var result = new List<T>();
            
            foreach (var dbName in _router.GetAllDatabaseNames())
            {
                if (!_databaseConfigs.TryGetValue(dbName, out var dbConfig))
                {
                    _logger?.LogWarning($"跳过未配置的数据库：{dbName}");
                    continue;
                }

                try
                {
                    using var connection = _connectionFactory.CreateConnection(dbConfig.ConnectionString, dbConfig.DbType);
                    connection.Open();
                    var data = connection.Query<T>(sql, param);
                    result.AddRange(data);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"在数据库 {dbName} 上执行查询时出错");
                }
            }

            return result;
        }

        /// <summary>
        /// 执行跨分片操作
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns>影响行数</returns>
        public int ExecuteAcrossShards(string sql, object param = null)
        {
            int totalAffected = 0;
            
            foreach (var dbName in _router.GetAllDatabaseNames())
            {
                if (!_databaseConfigs.TryGetValue(dbName, out var dbConfig))
                {
                    _logger?.LogWarning($"跳过未配置的数据库：{dbName}");
                    continue;
                }

                try
                {
                    using var connection = _connectionFactory.CreateConnection(dbConfig.ConnectionString, dbConfig.DbType);
                    connection.Open();
                    var affected = connection.Execute(sql, param);
                    totalAffected += affected;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"在数据库 {dbName} 上执行操作时出错");
                }
            }

            return totalAffected;
        }

        /// <summary>
        /// 开始分布式事务
        /// </summary>
        /// <returns>分布式事务</returns>
        public IShardingTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var transaction = new ShardingTransaction();
            
            foreach (var dbName in _router.GetAllDatabaseNames())
            {
                if (!_databaseConfigs.TryGetValue(dbName, out var dbConfig))
                {
                    _logger?.LogWarning($"跳过未配置的数据库：{dbName}");
                    continue;
                }

                try
                {
                    var connection = _connectionFactory.CreateConnection(dbConfig.ConnectionString, dbConfig.DbType);
                    connection.Open();
                    var dbTransaction = connection.BeginTransaction(isolationLevel);
                    transaction.AddTransaction(dbName, dbTransaction);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"在数据库 {dbName} 上开始事务时出错");
                    transaction.Dispose();
                    throw;
                }
            }

            return transaction;
        }
    }

    /// <summary>
    /// 数据库配置
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DbType { get; set; }
    }
} 