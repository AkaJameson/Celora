using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// 表管理器实现
    /// </summary>
    public class TableManager : ITableManager
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly Dictionary<string, DatabaseConfig> _databaseConfigs;
        private readonly ILogger<TableManager> _logger;
        private readonly Dictionary<string, HashSet<string>> _tableCache = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionFactory">连接工厂</param>
        /// <param name="databaseConfigs">数据库配置</param>
        /// <param name="logger">日志记录器</param>
        public TableManager(
            IDbConnectionFactory connectionFactory,
            Dictionary<string, DatabaseConfig> databaseConfigs,
            ILogger<TableManager> logger = null)
        {
            _connectionFactory = connectionFactory;
            _databaseConfigs = databaseConfigs;
            _logger = logger;
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public bool TableExists(string tableName, string dbName)
        {
            if (_tableCache.TryGetValue(dbName, out var tables))
            {
                return tables.Contains(tableName);
            }

            var allTables = GetAllTables(dbName);
            return allTables.Contains(tableName);
        }

        /// <summary>
        /// 异步检查表是否存在
        /// </summary>
        public async Task<bool> TableExistsAsync(string tableName, string dbName)
        {
            if (_tableCache.TryGetValue(dbName, out var tables))
            {
                return tables.Contains(tableName);
            }

            var allTables = await GetAllTablesAsync(dbName);
            return allTables.Contains(tableName);
        }

        /// <summary>
        /// 创建表
        /// </summary>
        public bool CreateTable(ITableDefinition tableDefinition, string tableName, string dbName)
        {
            try
            {
                if (!_databaseConfigs.TryGetValue(dbName, out var dbConfig))
                {
                    _logger?.LogWarning($"未找到数据库配置：{dbName}");
                    return false;
                }

                using var connection = _connectionFactory.CreateConnection(dbConfig.ConnectionString, dbConfig.DbType);
                connection.Open();

                var sql = tableDefinition.GenerateCreateTableSql(tableName, dbConfig.DbType);
                connection.Execute(sql);

                InvalidateTableCache(dbName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"创建表 {tableName} 在数据库 {dbName} 中出错");
                return false;
            }
        }

        /// <summary>
        /// 异步创建表
        /// </summary>
        public async Task<bool> CreateTableAsync(ITableDefinition tableDefinition, string tableName, string dbName)
        {
            try
            {
                if (!_databaseConfigs.TryGetValue(dbName, out var dbConfig))
                {
                    _logger?.LogWarning($"未找到数据库配置：{dbName}");
                    return false;
                }

                using var connection = _connectionFactory.CreateConnection(dbConfig.ConnectionString, dbConfig.DbType);
                await Task.Run(() => connection.Open());

                var sql = tableDefinition.GenerateCreateTableSql(tableName, dbConfig.DbType);
                connection.Execute(sql);

                InvalidateTableCache(dbName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"创建表 {tableName} 在数据库 {dbName} 中出错");
                return false;
            }
        }

        /// <summary>
        /// 创建表（如果不存在）
        /// </summary>
        public bool CreateTableIfNotExists(ITableDefinition tableDefinition, string tableName, string dbName)
        {
            if (TableExists(tableName, dbName))
            {
                return true;
            }

            return CreateTable(tableDefinition, tableName, dbName);
        }

        /// <summary>
        /// 异步创建表（如果不存在）
        /// </summary>
        public async Task<bool> CreateTableIfNotExistsAsync(ITableDefinition tableDefinition, string tableName, string dbName)
        {
            if (await TableExistsAsync(tableName, dbName))
            {
                return true;
            }

            return await CreateTableAsync(tableDefinition, tableName, dbName);
        }

        /// <summary>
        /// 获取所有表名
        /// </summary>
        public IEnumerable<string> GetAllTables(string dbName)
        {
            if (_tableCache.TryGetValue(dbName, out var tables))
            {
                return tables;
            }

            if (!_databaseConfigs.TryGetValue(dbName, out var dbConfig))
            {
                _logger?.LogWarning($"未找到数据库配置：{dbName}");
                return Enumerable.Empty<string>();
            }

            try
            {
                using var connection = _connectionFactory.CreateConnection(dbConfig.ConnectionString, dbConfig.DbType);
                connection.Open();

                var result = QueryAllTables(connection, dbConfig.DbType);

                // 缓存表名
                _tableCache[dbName] = new HashSet<string>(result);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"获取数据库 {dbName} 的所有表出错");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// 异步获取所有表名
        /// </summary>
        public async Task<IEnumerable<string>> GetAllTablesAsync(string dbName)
        {
            if (_tableCache.TryGetValue(dbName, out var tables))
            {
                return tables;
            }

            if (!_databaseConfigs.TryGetValue(dbName, out var dbConfig))
            {
                _logger?.LogWarning($"未找到数据库配置：{dbName}");
                return Enumerable.Empty<string>();
            }

            try
            {
                using var connection = _connectionFactory.CreateConnection(dbConfig.ConnectionString, dbConfig.DbType);
                await Task.Run(() => connection.Open());

                var result = await QueryAllTablesAsync(connection, dbConfig.DbType);

                // 缓存表名
                _tableCache[dbName] = new HashSet<string>(result);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"获取数据库 {dbName} 的所有表出错");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// 使表缓存失效
        /// </summary>
        /// <param name="dbName">数据库名</param>
        private void InvalidateTableCache(string dbName)
        {
            if (_tableCache.ContainsKey(dbName))
            {
                _tableCache.Remove(dbName);
            }
        }

        /// <summary>
        /// 查询所有表
        /// </summary>
        private IEnumerable<string> QueryAllTables(Core.IDbConnection connection, DatabaseType dbType)
        {
            string sql = dbType switch
            {
                DatabaseType.SQLite => "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';",
                DatabaseType.MySQL => "SHOW TABLES;",
                DatabaseType.PostgreSQL => "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';",
                DatabaseType.SQLServer => "SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE';",
                _ => throw new NotSupportedException($"不支持的数据库类型：{dbType}")
            };

            if (dbType == DatabaseType.MySQL)
            {
                var tables = connection.Query<string>(sql);
                return tables;
            }
            else
            {
                var tables = connection.Query<string>(sql);
                return tables;
            }
        }

        /// <summary>
        /// 异步查询所有表
        /// </summary>
        private async Task<IEnumerable<string>> QueryAllTablesAsync(Core.IDbConnection connection, DatabaseType dbType)
        {
            string sql = dbType switch
            {
                DatabaseType.SQLite => "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';",
                DatabaseType.MySQL => "SHOW TABLES;",
                DatabaseType.PostgreSQL => "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';",
                DatabaseType.SQLServer => "SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE';",
                _ => throw new NotSupportedException($"不支持的数据库类型：{dbType}")
            };

            if (dbType == DatabaseType.MySQL)
            {
                var tables = await Task.Run(() => connection.Query<string>(sql));
                return tables;
            }
            else
            {
                var tables = await Task.Run(() => connection.Query<string>(sql));
                return tables;
            }
        }
    }
}