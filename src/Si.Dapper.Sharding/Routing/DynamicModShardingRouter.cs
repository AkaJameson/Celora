using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Routing
{
    /// <summary>
    /// 基于取模的动态分片路由
    /// </summary>
    public class DynamicModShardingRouter : DynamicShardingRouter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="tableShardCount">每个库中的分表数量</param>
        /// <param name="tableManager">表管理器</param>
        /// <param name="tableDefinitions">表定义字典</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}</param>
        public DynamicModShardingRouter(
            string[] databaseNames, 
            int tableShardCount, 
            ITableManager tableManager, 
            Dictionary<string, ITableDefinition> tableDefinitions, 
            ILogger<DynamicModShardingRouter> logger = null, 
            string tableShardFormat = "{0}_{1}")
            : base(databaseNames, tableShardCount, tableManager, tableDefinitions, logger, tableShardFormat)
        {
        }

        /// <summary>
        /// 获取分片键的哈希值
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>哈希值</returns>
        protected override int GetShardKeyHashCode(object shardKey)
        {
            if (shardKey == null)
            {
                throw new ArgumentNullException(nameof(shardKey), "分片键不能为空");
            }

            if (shardKey is int intValue)
            {
                return Math.Abs(intValue);
            }
            else if (shardKey is long longValue)
            {
                return Math.Abs((int)(longValue % int.MaxValue));
            }
            else if (shardKey is short shortValue)
            {
                return Math.Abs(shortValue);
            }
            else if (shardKey is string stringValue && long.TryParse(stringValue, out var parsedValue))
            {
                return Math.Abs((int)(parsedValue % int.MaxValue));
            }
            else
            {
                throw new ArgumentException($"不支持的分片键类型：{shardKey.GetType().Name}，本路由仅支持数字类型的分片键");
            }
        }
    }
} 