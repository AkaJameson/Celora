using System;
using System.Collections.Generic;
using System.Reflection;

namespace Si.EntityFramework.AutoMigration.Models
{
    /// <summary>
    /// 表定义
    /// </summary>
    public class TableDefinition
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 模式名(SqlServer)
        /// </summary>
        public string Schema { get; set; }
        
        /// <summary>
        /// 列定义列表
        /// </summary>
        public List<ColumnDefinition> Columns { get; set; } = new List<ColumnDefinition>();
        
        /// <summary>
        /// 获取完整表名
        /// </summary>
        public string GetFullName()
        {
            return string.IsNullOrEmpty(Schema) ? Name : $"{Schema}.{Name}";
        }
    }
    
    /// <summary>
    /// 列定义
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }
        
        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool IsNullable { get; set; }
        
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        
        /// <summary>
        /// 是否为自增列
        /// </summary>
        public bool IsIdentity { get; set; }
        
        /// <summary>
        /// 最大长度
        /// </summary>
        public int? MaxLength { get; set; }
        
        /// <summary>
        /// 精度
        /// </summary>
        public int? Precision { get; set; }
        
        /// <summary>
        /// 小数位数
        /// </summary>
        public int? Scale { get; set; }
        
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
    }
    
    /// <summary>
    /// 实体定义
    /// </summary>
    public class EntityDefinition
    {
        /// <summary>
        /// 实体类型
        /// </summary>
        public Type ClrType { get; set; }
        
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        
        /// <summary>
        /// 模式名
        /// </summary>
        public string Schema { get; set; }
        
        /// <summary>
        /// 属性定义列表
        /// </summary>
        public List<PropertyDefinition> Properties { get; set; } = new List<PropertyDefinition>();
        
        /// <summary>
        /// 获取完整表名
        /// </summary>
        public string GetFullName()
        {
            return string.IsNullOrEmpty(Schema) ? TableName : $"{Schema}.{TableName}";
        }
    }
    
    /// <summary>
    /// 属性定义
    /// </summary>
    public class PropertyDefinition
    {
        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
        
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
        
        /// <summary>
        /// CLR类型
        /// </summary>
        public Type ClrType { get; set; }
        
        /// <summary>
        /// 是否必需
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        
        /// <summary>
        /// 是否为自增列
        /// </summary>
        public bool IsIdentity { get; set; }
        
        /// <summary>
        /// 最大长度
        /// </summary>
        public int? MaxLength { get; set; }
        
        /// <summary>
        /// 精度
        /// </summary>
        public int? Precision { get; set; }
        
        /// <summary>
        /// 小数位数
        /// </summary>
        public int? Scale { get; set; }
        
        /// <summary>
        /// 获取对应的SQL类型名称
        /// </summary>
        public string GetSqlType(DatabaseProviderType providerType)
        {
            Type type = Nullable.GetUnderlyingType(ClrType) ?? ClrType;
            
            if (providerType == DatabaseProviderType.SqlServer)
            {
                return GetSqlServerTypeName(type);
            }
            else if (providerType == DatabaseProviderType.Sqlite)
            {
                return GetSqliteTypeName(type);
            }
            else if (providerType == DatabaseProviderType.MySql)
            {
                return GetMySqlTypeName(type);
            }
            else if (providerType == DatabaseProviderType.PostgreSql)
            {
                return GetPostgreSqlTypeName(type);
            }
            
            return "NVARCHAR(100)";
        }
        
        private string GetSqlServerTypeName(Type type)
        {
            if (type == typeof(int))
            {
                return "INT";
            }
            else if (type == typeof(long))
            {
                return "BIGINT";
            }
            else if (type == typeof(short))
            {
                return "SMALLINT";
            }
            else if (type == typeof(byte))
            {
                return "TINYINT";
            }
            else if (type == typeof(bool))
            {
                return "BIT";
            }
            else if (type == typeof(decimal))
            {
                if (Precision.HasValue && Scale.HasValue)
                {
                    return $"DECIMAL({Precision.Value},{Scale.Value})";
                }
                return "DECIMAL(18,2)";
            }
            else if (type == typeof(float))
            {
                return "REAL";
            }
            else if (type == typeof(double))
            {
                return "FLOAT";
            }
            else if (type == typeof(DateTime))
            {
                return "DATETIME2";
            }
            else if (type == typeof(DateTimeOffset))
            {
                return "DATETIMEOFFSET";
            }
            else if (type == typeof(TimeSpan))
            {
                return "TIME";
            }
            else if (type == typeof(Guid))
            {
                return "UNIQUEIDENTIFIER";
            }
            else if (type == typeof(string))
            {
                if (MaxLength.HasValue && MaxLength.Value > 0)
                {
                    return $"NVARCHAR({MaxLength.Value})";
                }
                return "NVARCHAR(MAX)";
            }
            else if (type == typeof(byte[]))
            {
                if (MaxLength.HasValue && MaxLength.Value > 0)
                {
                    return $"VARBINARY({MaxLength.Value})";
                }
                return "VARBINARY(MAX)";
            }
            
            return "NVARCHAR(100)";
        }
        
        private string GetSqliteTypeName(Type type)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            {
                return "INTEGER";
            }
            else if (type == typeof(bool))
            {
                return "INTEGER"; // SQLite 没有布尔类型，用 INTEGER 代替
            }
            else if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
            {
                return "REAL";
            }
            else if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan))
            {
                return "TEXT"; // SQLite 没有日期类型，用 TEXT 代替
            }
            else if (type == typeof(Guid))
            {
                return "TEXT";
            }
            else if (type == typeof(string))
            {
                return "TEXT";
            }
            else if (type == typeof(byte[]))
            {
                return "BLOB";
            }
            
            return "TEXT";
        }
        
        private string GetMySqlTypeName(Type type)
        {
            if (type == typeof(int))
            {
                return "INT";
            }
            else if (type == typeof(long))
            {
                return "BIGINT";
            }
            else if (type == typeof(short))
            {
                return "SMALLINT";
            }
            else if (type == typeof(byte))
            {
                return "TINYINT";
            }
            else if (type == typeof(bool))
            {
                return "TINYINT(1)";
            }
            else if (type == typeof(decimal))
            {
                if (Precision.HasValue && Scale.HasValue)
                {
                    return $"DECIMAL({Precision.Value},{Scale.Value})";
                }
                return "DECIMAL(18,2)";
            }
            else if (type == typeof(float))
            {
                return "FLOAT";
            }
            else if (type == typeof(double))
            {
                return "DOUBLE";
            }
            else if (type == typeof(DateTime))
            {
                return "DATETIME";
            }
            else if (type == typeof(DateTimeOffset))
            {
                return "DATETIME";
            }
            else if (type == typeof(TimeSpan))
            {
                return "TIME";
            }
            else if (type == typeof(Guid))
            {
                return "CHAR(36)";
            }
            else if (type == typeof(string))
            {
                if (MaxLength.HasValue && MaxLength.Value > 0)
                {
                    if (MaxLength.Value <= 255)
                    {
                        return $"VARCHAR({MaxLength.Value})";
                    }
                    else if (MaxLength.Value <= 65535)
                    {
                        return $"TEXT";
                    }
                    else if (MaxLength.Value <= 16777215)
                    {
                        return "MEDIUMTEXT";
                    }
                    else
                    {
                        return "LONGTEXT";
                    }
                }
                return "LONGTEXT";
            }
            else if (type == typeof(byte[]))
            {
                if (MaxLength.HasValue && MaxLength.Value > 0)
                {
                    if (MaxLength.Value <= 255)
                    {
                        return $"VARBINARY({MaxLength.Value})";
                    }
                    else if (MaxLength.Value <= 65535)
                    {
                        return $"BLOB";
                    }
                    else if (MaxLength.Value <= 16777215)
                    {
                        return "MEDIUMBLOB";
                    }
                    else
                    {
                        return "LONGBLOB";
                    }
                }
                return "LONGBLOB";
            }
            
            return "VARCHAR(100)";
        }
        
        private string GetPostgreSqlTypeName(Type type)
        {
            if (type == typeof(int))
            {
                return "INTEGER";
            }
            else if (type == typeof(long))
            {
                return "BIGINT";
            }
            else if (type == typeof(short))
            {
                return "SMALLINT";
            }
            else if (type == typeof(byte))
            {
                return "SMALLINT";
            }
            else if (type == typeof(bool))
            {
                return "BOOLEAN";
            }
            else if (type == typeof(decimal))
            {
                if (Precision.HasValue && Scale.HasValue)
                {
                    return $"NUMERIC({Precision.Value},{Scale.Value})";
                }
                return "NUMERIC(18,2)";
            }
            else if (type == typeof(float))
            {
                return "REAL";
            }
            else if (type == typeof(double))
            {
                return "DOUBLE PRECISION";
            }
            else if (type == typeof(DateTime))
            {
                return "TIMESTAMP";
            }
            else if (type == typeof(DateTimeOffset))
            {
                return "TIMESTAMP WITH TIME ZONE";
            }
            else if (type == typeof(TimeSpan))
            {
                return "INTERVAL";
            }
            else if (type == typeof(Guid))
            {
                return "UUID";
            }
            else if (type == typeof(string))
            {
                if (MaxLength.HasValue && MaxLength.Value > 0)
                {
                    return $"VARCHAR({MaxLength.Value})";
                }
                return "TEXT";
            }
            else if (type == typeof(byte[]))
            {
                return "BYTEA";
            }
            
            return "VARCHAR(100)";
        }
    }
    
    /// <summary>
    /// 迁移脚本
    /// </summary>
    public class MigrationScript
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        
        /// <summary>
        /// 操作类型
        /// </summary>
        public string Operation { get; set; }
        
        /// <summary>
        /// SQL脚本
        /// </summary>
        public string Sql { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
    
    /// <summary>
    /// 模式差异
    /// </summary>
    public class SchemaDifference
    {
        /// <summary>
        /// 需要创建的表
        /// </summary>
        public List<EntityDefinition> TablesToCreate { get; set; } = new List<EntityDefinition>();
        
        /// <summary>
        /// 需要删除的表
        /// </summary>
        public List<TableDefinition> TablesToDelete { get; set; } = new List<TableDefinition>();
        
        /// <summary>
        /// 表变更列表
        /// </summary>
        public List<TableChange> TableChanges { get; set; } = new List<TableChange>();
        
        /// <summary>
        /// 是否有变更
        /// </summary>
        public bool HasChanges => 
            TablesToCreate.Count > 0 || 
            TablesToDelete.Count > 0 || 
            TableChanges.Count > 0;
    }
    
    /// <summary>
    /// 表变更
    /// </summary>
    public class TableChange
    {
        /// <summary>
        /// 实体定义
        /// </summary>
        public EntityDefinition Entity { get; set; }
        
        /// <summary>
        /// 表定义
        /// </summary>
        public TableDefinition Table { get; set; }
        
        /// <summary>
        /// 需要添加的列
        /// </summary>
        public List<PropertyDefinition> ColumnsToAdd { get; set; } = new List<PropertyDefinition>();
        
        /// <summary>
        /// 需要修改的列
        /// </summary>
        public List<ColumnPropertyPair> ColumnsToAlter { get; set; } = new List<ColumnPropertyPair>();
        
        /// <summary>
        /// 需要删除的列
        /// </summary>
        public List<ColumnDefinition> ColumnsToDelete { get; set; } = new List<ColumnDefinition>();
        
        /// <summary>
        /// 是否有变更
        /// </summary>
        public bool HasChanges => 
            ColumnsToAdd.Count > 0 || 
            ColumnsToAlter.Count > 0 || 
            ColumnsToDelete.Count > 0;
    }
    
    /// <summary>
    /// 列-属性对应关系
    /// </summary>
    public class ColumnPropertyPair
    {
        /// <summary>
        /// 数据库列
        /// </summary>
        public ColumnDefinition Column { get; set; }
        
        /// <summary>
        /// 实体属性
        /// </summary>
        public PropertyDefinition Property { get; set; }
    }
} 