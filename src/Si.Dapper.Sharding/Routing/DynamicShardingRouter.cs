using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Routing
{
    /// <summary>
    /// 动态分片路由
    /// </summary>
    public class DynamicShardingRouter : IShardingRouter
    {
        private readonly string[] _databaseNames;
        private readonly int _tableShardCount;
        private readonly string _tableShardFormat;
        private readonly ITableManager _tableManager;
        private readonly Dictionary<string, ITableDefinition> _tableDefinitions;
        private readonly ILogger<DynamicShardingRouter> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="tableShardCount">每个库中的分表数量</param>
        /// <param name="tableManager">表管理器</param>
        /// <param name="tableDefinitions">表定义字典</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}，其中{0}为基础表名，{1}为分表索引</param>
        public DynamicShardingRouter(
            string[] databaseNames, 
            int tableShardCount,
            ITableManager tableManager,
            Dictionary<string, ITableDefinition> tableDefinitions,
            ILogger<DynamicShardingRouter> logger = null,
            string tableShardFormat = "{0}_{1}")
        {
            if (databaseNames == null || databaseNames.Length == 0)
            {
                throw new ArgumentException("数据库名称列表不能为空");
            }

            if (tableShardCount <= 0)
            {
                throw new ArgumentException("分表数量必须大于0");
            }

            _databaseNames = databaseNames;
            _tableShardCount = tableShardCount;
            _tableShardFormat = tableShardFormat;
            _tableManager = tableManager;
            _tableDefinitions = tableDefinitions;
            _logger = logger;
        }

        /// <summary>
        /// 根据分片键获取数据库名称
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>数据库名称</returns>
        public string GetDatabaseName(object shardKey)
        {
            var hash = Math.Abs(GetShardKeyHashCode(shardKey));
            var dbIndex = hash % _databaseNames.Length;
            return _databaseNames[dbIndex];
        }

        /// <summary>
        /// 根据分片键获取表名，如果表不存在则创建
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <param name="baseTableName">基础表名</param>
        /// <returns>分片表名</returns>
        public string GetTableName(object shardKey, string baseTableName)
        {
            var hash = Math.Abs(GetShardKeyHashCode(shardKey));
            var tableIndex = hash % _tableShardCount;
            var tableName = string.Format(_tableShardFormat, baseTableName, tableIndex);
            var dbName = GetDatabaseName(shardKey);
            
            // 检查表是否存在，不存在则创建
            EnsureTableExists(tableName, baseTableName, dbName);
            
            return tableName;
        }

        /// <summary>
        /// 获取所有数据库名称
        /// </summary>
        /// <returns>数据库名称列表</returns>
        public IEnumerable<string> GetAllDatabaseNames()
        {
            return _databaseNames;
        }

        /// <summary>
        /// 获取指定基础表的所有分片表名，并确保这些表存在
        /// </summary>
        /// <param name="baseTableName">基础表名</param>
        /// <returns>分片表名列表</returns>
        public IEnumerable<string> GetAllTableNames(string baseTableName)
        {
            var tableNames = new List<string>();
            
            for (int i = 0; i < _tableShardCount; i++)
            {
                var tableName = string.Format(_tableShardFormat, baseTableName, i);
                tableNames.Add(tableName);
                
                // 确保所有数据库中都有这个表
                foreach (var dbName in _databaseNames)
                {
                    EnsureTableExists(tableName, baseTableName, dbName);
                }
            }
            
            return tableNames;
        }

        /// <summary>
        /// 确保表存在
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="baseTableName">基础表名</param>
        /// <param name="dbName">数据库名</param>
        protected virtual void EnsureTableExists(string tableName, string baseTableName, string dbName)
        {
            if (!_tableDefinitions.TryGetValue(baseTableName, out var tableDefinition))
            {
                _logger?.LogWarning($"未找到表定义：{baseTableName}");
                return;
            }
            
            if (!_tableManager.TableExists(tableName, dbName))
            {
                _logger?.LogInformation($"正在创建表 {tableName} 在数据库 {dbName} 中");
                var success = _tableManager.CreateTable(tableDefinition, tableName, dbName);
                if (!success)
                {
                    _logger?.LogError($"创建表 {tableName} 在数据库 {dbName} 中失败");
                }
            }
        }

        /// <summary>
        /// 获取分片键的哈希值
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>哈希值</returns>
        protected virtual int GetShardKeyHashCode(object shardKey)
        {
            if (shardKey == null)
            {
                throw new ArgumentNullException(nameof(shardKey), "分片键不能为空");
            }
            
            return shardKey.GetHashCode();
        }
    }
} 