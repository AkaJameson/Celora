using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Routing
{
    /// <summary>
    /// 基于取模的分片路由，适用于数字类型分片键
    /// </summary>
    public class ModShardingRouter : IShardingRouter
    {
        private readonly string[] _databaseNames;
        private readonly int _tableShardCount;
        private readonly string _tableShardFormat;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseNames">数据库名称列表</param>
        /// <param name="tableShardCount">每个库中的分表数量</param>
        /// <param name="tableShardFormat">分表名称格式，默认为 {0}_{1}，其中{0}为基础表名，{1}为分表索引</param>
        public ModShardingRouter(string[] databaseNames, int tableShardCount, string tableShardFormat = "{0}_{1}")
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
        }

        /// <summary>
        /// 根据分片键获取数据库名称
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>数据库名称</returns>
        public string GetDatabaseName(object shardKey)
        {
            var shardValue = GetShardKeyValue(shardKey);
            var dbIndex = shardValue % _databaseNames.Length;
            return _databaseNames[dbIndex];
        }

        /// <summary>
        /// 根据分片键获取表名
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <param name="baseTableName">基础表名</param>
        /// <returns>分片表名</returns>
        public string GetTableName(object shardKey, string baseTableName)
        {
            var shardValue = GetShardKeyValue(shardKey);
            var tableIndex = shardValue % _tableShardCount;
            return string.Format(_tableShardFormat, baseTableName, tableIndex);
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
            for (int i = 0; i < _tableShardCount; i++)
            {
                yield return string.Format(_tableShardFormat, baseTableName, i);
            }
        }

        /// <summary>
        /// 获取分片键的值
        /// </summary>
        /// <param name="shardKey">分片键</param>
        /// <returns>用于取模的值</returns>
        protected virtual long GetShardKeyValue(object shardKey)
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
                return Math.Abs(longValue);
            }
            else if (shardKey is short shortValue)
            {
                return Math.Abs(shortValue);
            }
            else if (shardKey is string stringValue && long.TryParse(stringValue, out var parsedValue))
            {
                return Math.Abs(parsedValue);
            }
            else
            {
                throw new ArgumentException($"不支持的分片键类型：{shardKey.GetType().Name}，本路由仅支持数字类型的分片键");
            }
        }
    }
} 