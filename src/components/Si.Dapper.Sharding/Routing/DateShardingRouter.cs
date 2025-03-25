using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Routing
{
    /// <summary>
    /// 日期分表周期
    /// </summary>
    public enum DateShardingPeriod
    {
        /// <summary>
        /// 按天分表
        /// </summary>
        Day,
        
        /// <summary>
        /// 按月分表
        /// </summary>
        Month,
        
        /// <summary>
        /// 按年分表
        /// </summary>
        Year
    }
    
    /// <summary>
    /// 基于日期的动态分片路由
    /// </summary>
    public class DateShardingRouter : IShardingRouter
    {
        private readonly string[] _databaseNames;
        private readonly string _tableShardFormat;
        private readonly DateShardingPeriod _shardingPeriod;
        private readonly ITableManager _tableManager;
        private readonly Dictionary<string, ITableDefinition> _tableDefinitions;
        private readonly ILogger<DateShardingRouter> _logger;
        private readonly int _historyTableCount;
        private readonly int _futureTableCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="tableManager">表管理器</param>
        /// <param name="tableDefinitions">表定义字典</param>
        /// <param name="shardingPeriod">分表周期</param>
        /// <param name="historyTableCount">要创建的历史表数量</param>
        /// <param name="futureTableCount">要创建的未来表数量</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}，其中{0}为基础表名，{1}为日期</param>
        public DateShardingRouter(
            string[] databaseNames,
            ITableManager tableManager,
            Dictionary<string, ITableDefinition> tableDefinitions,
            DateShardingPeriod shardingPeriod = DateShardingPeriod.Month,
            int historyTableCount = 3,
            int futureTableCount = 1,
            ILogger<DateShardingRouter> logger = null,
            string tableShardFormat = "{0}_{1}")
        {
            if (databaseNames == null || databaseNames.Length == 0)
            {
                throw new ArgumentException("数据库名称列表不能为空");
            }

            _databaseNames = databaseNames;
            _tableShardFormat = tableShardFormat;
            _shardingPeriod = shardingPeriod;
            _tableManager = tableManager;
            _tableDefinitions = tableDefinitions;
            _logger = logger;
            _historyTableCount = historyTableCount;
            _futureTableCount = futureTableCount;
        }

        /// <summary>
        /// 根据分片键获取数据库名称
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>数据库名称</returns>
        public string GetDatabaseName(object shardKey)
        {
            // 对于日期分表，我们通常只按表分，不按库分
            // 这里简单地按照哈希分配数据库
            var hash = Math.Abs(shardKey.GetHashCode());
            var dbIndex = hash % _databaseNames.Length;
            return _databaseNames[dbIndex];
        }

        /// <summary>
        /// 根据分片键获取表名
        /// </summary>
        /// <param name="shardKey">分片键（日期类型）</param>
        /// <param name="baseTableName">基础表名</param>
        /// <returns>分片表名</returns>
        public string GetTableName(object shardKey, string baseTableName)
        {
            DateTime date = GetDateFromShardKey(shardKey);
            string dateSuffix = FormatDateSuffix(date);
            string tableName = string.Format(_tableShardFormat, baseTableName, dateSuffix);
            
            // 检查表是否存在，不存在则创建
            string dbName = GetDatabaseName(shardKey);
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
        /// 获取指定基础表的所有分片表名
        /// </summary>
        /// <param name="baseTableName">基础表名</param>
        /// <returns>分片表名列表</returns>
        public IEnumerable<string> GetAllTableNames(string baseTableName)
        {
            var now = DateTime.Now;
            var tableNames = new List<string>();
            
            // 创建历史表、当前表和未来表
            for (int i = -_historyTableCount; i <= _futureTableCount; i++)
            {
                DateTime date = GetDateWithOffset(now, i);
                string dateSuffix = FormatDateSuffix(date);
                string tableName = string.Format(_tableShardFormat, baseTableName, dateSuffix);
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
        /// 立即预创建未来表
        /// </summary>
        /// <param name="baseTableName">基础表名</param>
        /// <param name="monthsAhead">预创建的月数</param>
        public void PrecreateFutureTables(string baseTableName, int monthsAhead)
        {
            var now = DateTime.Now;
            
            for (int i = 1; i <= monthsAhead; i++)
            {
                DateTime date = GetDateWithOffset(now, i);
                string dateSuffix = FormatDateSuffix(date);
                string tableName = string.Format(_tableShardFormat, baseTableName, dateSuffix);
                
                foreach (var dbName in _databaseNames)
                {
                    EnsureTableExists(tableName, baseTableName, dbName);
                }
            }
        }

        /// <summary>
        /// 确保表存在
        /// </summary>
        private void EnsureTableExists(string tableName, string baseTableName, string dbName)
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
        /// 从分片键获取日期
        /// </summary>
        private DateTime GetDateFromShardKey(object shardKey)
        {
            if (shardKey is DateTime dateTime)
            {
                return dateTime;
            }
            else if (shardKey is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.DateTime;
            }
            else if (shardKey is string dateString && DateTime.TryParse(dateString, out var parsedDate))
            {
                return parsedDate;
            }
            else
            {
                throw new ArgumentException($"无法从分片键获取日期：{shardKey}");
            }
        }
        
        /// <summary>
        /// 格式化日期后缀
        /// </summary>
        private string FormatDateSuffix(DateTime date)
        {
            return _shardingPeriod switch
            {
                DateShardingPeriod.Day => date.ToString("yyyyMMdd"),
                DateShardingPeriod.Month => date.ToString("yyyyMM"),
                DateShardingPeriod.Year => date.ToString("yyyy"),
                _ => throw new NotSupportedException($"不支持的分表周期：{_shardingPeriod}")
            };
        }
        
        /// <summary>
        /// 获取偏移日期
        /// </summary>
        private DateTime GetDateWithOffset(DateTime baseDate, int offset)
        {
            return _shardingPeriod switch
            {
                DateShardingPeriod.Day => baseDate.AddDays(offset),
                DateShardingPeriod.Month => baseDate.AddMonths(offset),
                DateShardingPeriod.Year => baseDate.AddYears(offset),
                _ => throw new NotSupportedException($"不支持的分表周期：{_shardingPeriod}")
            };
        }
    }
} 