using System.Collections.Generic;

namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 表结构定义接口
    /// </summary>
    public interface ITableDefinition
    {
        /// <summary>
        /// 获取基础表名
        /// </summary>
        string BaseTableName { get; }
        
        /// <summary>
        /// 获取表字段定义
        /// </summary>
        IEnumerable<TableColumnDefinition> GetColumns();
        
        /// <summary>
        /// 获取主键定义
        /// </summary>
        TableColumnDefinition GetPrimaryKey();
        
        /// <summary>
        /// 获取索引定义
        /// </summary>
        IEnumerable<TableIndexDefinition> GetIndexes();
        
        /// <summary>
        /// 根据数据库类型生成建表SQL
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns>建表SQL语句</returns>
        string GenerateCreateTableSql(string tableName, DatabaseType dbType);
    }
    
    /// <summary>
    /// 表字段定义
    /// </summary>
    public class TableColumnDefinition
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 字段类型映射
        /// </summary>
        public Dictionary<DatabaseType, string> TypeMapping { get; set; } = new Dictionary<DatabaseType, string>();
        
        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool AllowNull { get; set; } = true;
        
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        
        /// <summary>
        /// 是否是自增字段
        /// </summary>
        public bool IsAutoIncrement { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }
    }
    
    /// <summary>
    /// 表索引定义
    /// </summary>
    public class TableIndexDefinition
    {
        /// <summary>
        /// 索引名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 索引字段
        /// </summary>
        public List<string> Columns { get; set; } = new List<string>();
        
        /// <summary>
        /// 是否唯一索引
        /// </summary>
        public bool IsUnique { get; set; }
    }
} 