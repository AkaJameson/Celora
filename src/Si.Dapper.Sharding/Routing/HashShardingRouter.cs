using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Routing
{
    /// <summary>
    /// 基于哈希的分片路由
    /// </summary>
    public class HashShardingRouter : IShardingRouter
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
        public HashShardingRouter(string[] databaseNames, int tableShardCount, string tableShardFormat = "{0}_{1}")
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
            var hash = Math.Abs(GetShardKeyHashCode(shardKey));
            var dbIndex = hash % _databaseNames.Length;
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
            var hash = Math.Abs(GetShardKeyHashCode(shardKey));
            var tableIndex = hash % _tableShardCount;
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