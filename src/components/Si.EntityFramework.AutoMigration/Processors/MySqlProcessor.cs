using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Si.EntityFramework.AutoMigration.Models;

namespace Si.EntityFramework.AutoMigration.Processors
{
    /// <summary>
    /// MySQL 处理器
    /// </summary>
    internal class MySqlProcessor : BaseDatabaseProcessor
    {
        public MySqlProcessor(DbContext context, AutoMigrationOptions options)
            : base(context, options, DatabaseProviderType.MySql)
        {
        }
        
        /// <summary>
        /// 确保迁移历史表存在
        /// </summary>
        public override async Task EnsureMigrationHistoryTableExistsAsync()
        {
            string sql = $@"
                CREATE TABLE IF NOT EXISTS {EscapeIdentifier(_options.HistoryTableName)} (
                    `Id` INT AUTO_INCREMENT PRIMARY KEY,
                    `TableName` VARCHAR(128) NOT NULL,
                    `Operation` VARCHAR(50) NOT NULL,
                    `SqlScript` TEXT NULL,
                    `AppliedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    `Description` VARCHAR(500) NULL
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ";
            
            await _context.Database.ExecuteSqlRawAsync(sql);
        }
        
        /// <summary>
        /// 获取数据库定义
        /// </summary>
        public override async Task<List<TableDefinition>> GetDatabaseDefinitionsAsync()
        {
            // 获取当前数据库名称
            string dbName = await GetCurrentDatabaseNameAsync();
            
            // 获取所有表
            var tables = await GetTablesAsync(dbName);
            
            // 获取所有列
            var columns = await GetColumnsAsync(dbName);
            
            // 处理列-表关系
            foreach (var table in tables)
            {
                table.Columns = columns.FindAll(c => 
                    c.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)
                );
            }
            
            return tables;
        }
        
        /// <summary>
        /// 获取当前数据库名称
        /// </summary>
        private async Task<string> GetCurrentDatabaseNameAsync()
        {
            string sql = "SELECT DATABASE()";
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                
                if (command.Connection.State != ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                
                return (await command.ExecuteScalarAsync())?.ToString();
            }
        }
        
        /// <summary>
        /// 获取所有表
        /// </summary>
        private async Task<List<TableDefinition>> GetTablesAsync(string dbName)
        {
            var tables = new List<TableDefinition>();
            
            string sql = $@"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = '{dbName}' 
                AND TABLE_TYPE = 'BASE TABLE'
                AND TABLE_NAME <> '{_options.HistoryTableName}'
                ORDER BY TABLE_NAME
            ";
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                
                if (command.Connection.State != ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tables.Add(new TableDefinition
                        {
                            Name = reader.GetString(0),
                            Schema = dbName, // MySQL 将数据库名称作为 Schema
                            Columns = new List<ColumnDefinition>()
                        });
                    }
                }
            }
            
            return tables;
        }
        
        /// <summary>
        /// 获取所有列
        /// </summary>
        private async Task<List<ColumnDefinition>> GetColumnsAsync(string dbName)
        {
            var columns = new List<ColumnDefinition>();
            
            string sql = $@"
                SELECT 
                    TABLE_NAME,
                    COLUMN_NAME,
                    DATA_TYPE,
                    CHARACTER_MAXIMUM_LENGTH,
                    NUMERIC_PRECISION,
                    NUMERIC_SCALE,
                    IS_NULLABLE,
                    COLUMN_KEY,
                    EXTRA
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = '{dbName}'
                AND TABLE_NAME <> '{_options.HistoryTableName}'
                ORDER BY TABLE_NAME, ORDINAL_POSITION
            ";
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                
                if (command.Connection.State != ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var column = new ColumnDefinition
                        {
                            Name = reader.GetString(1), // COLUMN_NAME
                            DataType = reader.GetString(2), // DATA_TYPE
                            MaxLength = !reader.IsDBNull(3) ? reader.GetInt32(3) : (int?)null, // CHARACTER_MAXIMUM_LENGTH
                            Precision = !reader.IsDBNull(4) ? reader.GetInt32(4) : (int?)null, // NUMERIC_PRECISION
                            Scale = !reader.IsDBNull(5) ? reader.GetInt32(5) : (int?)null, // NUMERIC_SCALE
                            IsNullable = reader.GetString(6) == "YES", // IS_NULLABLE
                            IsPrimaryKey = reader.GetString(7) == "PRI", // COLUMN_KEY
                            IsIdentity = reader.GetString(8).Contains("auto_increment") // EXTRA
                        };
                        
                        columns.Add(column);
                    }
                }
            }
            
            return columns;
        }
        
        /// <summary>
        /// 检查数据类型兼容性
        /// </summary>
        protected override bool IsCompatibleDataType(PropertyDefinition property, ColumnDefinition column)
        {
            string requiredType = property.GetSqlType(_providerType).ToUpperInvariant();
            string actualType = column.DataType.ToUpperInvariant();
            
            // 整数类型兼容检查
            if ((requiredType.Contains("INT") || requiredType.Contains("TINYINT") || 
                 requiredType.Contains("SMALLINT") || requiredType.Contains("MEDIUMINT") || 
                 requiredType.Contains("BIGINT")) &&
                (actualType.Contains("INT") || actualType.Contains("TINYINT") || 
                 actualType.Contains("SMALLINT") || actualType.Contains("MEDIUMINT") || 
                 actualType.Contains("BIGINT")))
            {
                // 对于整数类型，只要目标列能容纳当前列的值范围即可
                // 简化检查：如果需要更大的整数类型，认为不兼容
                if (GetIntegerTypeSize(requiredType) > GetIntegerTypeSize(actualType))
                {
                    return false;
                }
                return true;
            }
            
            // 字符串类型兼容检查
            if ((requiredType.Contains("CHAR") || requiredType.Contains("VARCHAR") || 
                 requiredType.Contains("TEXT") || requiredType.Contains("TINYTEXT") || 
                 requiredType.Contains("MEDIUMTEXT") || requiredType.Contains("LONGTEXT")) &&
                (actualType.Contains("CHAR") || actualType.Contains("VARCHAR") || 
                 actualType.Contains("TEXT") || actualType.Contains("TINYTEXT") || 
                 actualType.Contains("MEDIUMTEXT") || actualType.Contains("LONGTEXT")))
            {
                // 检查长度是否兼容
                if (property.MaxLength.HasValue && column.MaxLength.HasValue)
                {
                    // 如果需要更长的字符串长度，认为不兼容
                    return property.MaxLength <= column.MaxLength;
                }
                
                // TEXT类型自动兼容
                if (requiredType.Contains("TEXT") && actualType.Contains("TEXT"))
                {
                    // 检查TEXT类型大小
                    return GetTextTypeSize(requiredType) <= GetTextTypeSize(actualType);
                }
                
                return true;
            }
            
            // 浮点类型兼容检查
            if ((requiredType.Contains("FLOAT") || requiredType.Contains("DOUBLE") || 
                 requiredType.Contains("DECIMAL") || requiredType.Contains("NUMERIC")) &&
                (actualType.Contains("FLOAT") || actualType.Contains("DOUBLE") || 
                 actualType.Contains("DECIMAL") || actualType.Contains("NUMERIC")))
            {
                // 检查精度和小数位数是否兼容
                if (property.Precision.HasValue && property.Scale.HasValue && 
                    column.Precision.HasValue && column.Scale.HasValue)
                {
                    // 如果需要更高的精度或小数位数，认为不兼容
                    return property.Precision <= column.Precision && property.Scale <= column.Scale;
                }
                
                return true;
            }
            
            // 日期类型兼容检查
            if ((requiredType.Contains("DATE") || requiredType.Contains("TIME") || 
                 requiredType.Contains("DATETIME") || requiredType.Contains("TIMESTAMP")) &&
                (actualType.Contains("DATE") || actualType.Contains("TIME") || 
                 actualType.Contains("DATETIME") || actualType.Contains("TIMESTAMP")))
            {
                // 不同的日期类型可能不兼容
                return requiredType == actualType;
            }
            
            // 二进制类型兼容检查
            if ((requiredType.Contains("BLOB") || requiredType.Contains("BINARY") || 
                 requiredType.Contains("VARBINARY")) &&
                (actualType.Contains("BLOB") || actualType.Contains("BINARY") || 
                 actualType.Contains("VARBINARY")))
            {
                // 检查BLOB类型大小
                if (requiredType.Contains("BLOB") && actualType.Contains("BLOB"))
                {
                    return GetTextTypeSize(requiredType) <= GetTextTypeSize(actualType);
                }
                
                return true;
            }
            
            // 特殊类型兼容检查
            if (requiredType == actualType)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取整数类型大小
        /// </summary>
        private int GetIntegerTypeSize(string type)
        {
            if (type.Contains("TINYINT")) return 1;
            if (type.Contains("SMALLINT")) return 2;
            if (type.Contains("MEDIUMINT")) return 3;
            if (type.Contains("INT")) return 4;
            if (type.Contains("BIGINT")) return 8;
            return 0;
        }
        
        /// <summary>
        /// 获取TEXT/BLOB类型大小
        /// </summary>
        private int GetTextTypeSize(string type)
        {
            if (type.Contains("TINYTEXT") || type.Contains("TINYBLOB")) return 1;
            if (type.Contains("TEXT") || type.Contains("BLOB")) return 2;
            if (type.Contains("MEDIUMTEXT") || type.Contains("MEDIUMBLOB")) return 3;
            if (type.Contains("LONGTEXT") || type.Contains("LONGBLOB")) return 4;
            return 0;
        }
        
        /// <summary>
        /// 生成创建表的SQL脚本
        /// </summary>
        protected override string GenerateCreateTableScript(EntityDefinition entity)
        {
            var sql = new StringBuilder();
            
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS {EscapeIdentifier(entity.TableName)} (");
            
            // 列定义
            var columnDefinitions = new List<string>();
            var primaryKeys = new List<string>();
            
            foreach (var property in entity.Properties)
            {
                var columnDefinition = new StringBuilder();
                
                // 列名和类型
                columnDefinition.Append($"    {EscapeIdentifier(property.ColumnName)} {property.GetSqlType(_providerType)}");
                
                // 可空性
                if (property.IsRequired)
                {
                    columnDefinition.Append(" NOT NULL");
                }
                else
                {
                    columnDefinition.Append(" NULL");
                }
                
                // 自增
                if (property.IsIdentity)
                {
                    columnDefinition.Append(" AUTO_INCREMENT");
                }
                
                // 主键约束稍后添加
                if (property.IsPrimaryKey)
                {
                    primaryKeys.Add(property.ColumnName);
                }
                
                columnDefinitions.Add(columnDefinition.ToString());
            }
            
            // 添加主键约束
            if (primaryKeys.Count > 0)
            {
                var pkColumns = string.Join(", ", primaryKeys.Select(pk => EscapeIdentifier(pk)));
                columnDefinitions.Add($"    PRIMARY KEY ({pkColumns})");
            }
            
            // 拼接列定义
            sql.AppendLine(string.Join(",\r\n", columnDefinitions));
            sql.AppendLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;");
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成添加列的SQL脚本
        /// </summary>
        protected override string GenerateAddColumnScript(EntityDefinition entity, PropertyDefinition property)
        {
            var sql = new StringBuilder();
            
            sql.Append($"ALTER TABLE {EscapeIdentifier(entity.TableName)} ADD COLUMN {EscapeIdentifier(property.ColumnName)} {property.GetSqlType(_providerType)}");
            
            // 可空性
            if (property.IsRequired)
            {
                sql.Append(" NOT NULL");
            }
            else
            {
                sql.Append(" NULL");
            }
            
            // 自增
            if (property.IsIdentity)
            {
                sql.Append(" AUTO_INCREMENT");
            }
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成修改列的SQL脚本
        /// </summary>
        protected override string GenerateAlterColumnScript(EntityDefinition entity, ColumnPropertyPair columnPair)
        {
            var sql = new StringBuilder();
            
            sql.Append($"ALTER TABLE {EscapeIdentifier(entity.TableName)} MODIFY COLUMN {EscapeIdentifier(columnPair.Property.ColumnName)} {columnPair.Property.GetSqlType(_providerType)}");
            
            // 可空性
            if (columnPair.Property.IsRequired)
            {
                sql.Append(" NOT NULL");
            }
            else
            {
                sql.Append(" NULL");
            }
            
            // 自增
            if (columnPair.Property.IsIdentity)
            {
                sql.Append(" AUTO_INCREMENT");
            }
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成删除列的SQL脚本
        /// </summary>
        protected override string GenerateDropColumnScript(EntityDefinition entity, ColumnDefinition column)
        {
            return $"ALTER TABLE {EscapeIdentifier(entity.TableName)} DROP COLUMN {EscapeIdentifier(column.Name)}";
        }
        
        /// <summary>
        /// 生成删除表的SQL脚本
        /// </summary>
        protected override string GenerateDropTableScript(TableDefinition table)
        {
            return $"DROP TABLE IF EXISTS {EscapeIdentifier(table.Name)}";
        }
        
        /// <summary>
        /// 转义标识符
        /// </summary>
        protected override string EscapeIdentifier(string identifier)
        {
            // MySQL 使用反引号转义标识符
            return $"`{identifier.Replace("`", "``")}`";
        }
        
        /// <summary>
        /// 创建参数名
        /// </summary>
        protected override string CreateParameterName(string name)
        {
            return $"@{name}";
        }
        
        /// <summary>
        /// 创建参数
        /// </summary>
        protected override object CreateParameter(string name, object value)
        {
            return new MySqlParameter($"@{name}", value ?? DBNull.Value);
        }
        
        /// <summary>
        /// 获取当前日期时间函数
        /// </summary>
        protected override string GetCurrentDateTimeFunction()
        {
            return "NOW()";
        }
    }
} 