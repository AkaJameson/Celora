using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.AutoMigration.Models;

namespace Si.EntityFramework.AutoMigration.Processors
{
    /// <summary>
    /// SQL Server处理器
    /// </summary>
    internal class SqlServerProcessor : BaseDatabaseProcessor
    {
        public SqlServerProcessor(DbContext context, AutoMigrationOptions options)
            : base(context, options, DatabaseProviderType.SqlServer)
        {
        }
        
        /// <summary>
        /// 确保迁移历史表存在
        /// </summary>
        public override async Task EnsureMigrationHistoryTableExistsAsync()
        {
            string sql = $@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_options.HistoryTableName}')
                BEGIN
                    CREATE TABLE [{_options.HistoryTableName}] (
                        [Id] INT IDENTITY(1,1) PRIMARY KEY,
                        [TableName] NVARCHAR(128) NOT NULL,
                        [Operation] NVARCHAR(50) NOT NULL,
                        [SqlScript] NVARCHAR(MAX) NULL,
                        [AppliedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        [Description] NVARCHAR(500) NULL
                    )
                END
            ";
            
            await _context.Database.ExecuteSqlRawAsync(sql);
        }
        
        /// <summary>
        /// 获取数据库定义
        /// </summary>
        public override async Task<List<TableDefinition>> GetDatabaseDefinitionsAsync()
        {
            // 先获取所有表
            var tables = await GetTablesAsync();
            
            // 再获取所有列
            var columns = await GetColumnsAsync();
            
            // 处理列-表关系
            foreach (var table in tables)
            {
                table.Columns = columns.FindAll(c => 
                    c.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase) && 
                    (string.IsNullOrEmpty(table.Schema) || c.Name.Equals(table.Schema, StringComparison.OrdinalIgnoreCase))
                );
            }
            
            return tables;
        }
        
        /// <summary>
        /// 获取所有表
        /// </summary>
        private async Task<List<TableDefinition>> GetTablesAsync()
        {
            var tables = new List<TableDefinition>();
            
            string sql = $@"
                SELECT 
                    t.TABLE_SCHEMA AS TableSchema,
                    t.TABLE_NAME AS TableName
                FROM 
                    INFORMATION_SCHEMA.TABLES t
                WHERE 
                    t.TABLE_TYPE = 'BASE TABLE'
                    AND t.TABLE_NAME <> '{_options.HistoryTableName}'
                ORDER BY 
                    t.TABLE_SCHEMA, t.TABLE_NAME
            ";
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                
                if (command.Connection.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();
                    
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tables.Add(new TableDefinition
                        {
                            Schema = reader["TableSchema"].ToString(),
                            Name = reader["TableName"].ToString(),
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
        private async Task<List<ColumnDefinition>> GetColumnsAsync()
        {
            var columns = new List<ColumnDefinition>();
            
            string sql = $@"
                SELECT 
                    c.TABLE_SCHEMA AS TableSchema,
                    c.TABLE_NAME AS TableName,
                    c.COLUMN_NAME AS ColumnName,
                    c.DATA_TYPE AS DataType,
                    c.CHARACTER_MAXIMUM_LENGTH AS MaxLength,
                    c.NUMERIC_PRECISION AS Precision,
                    c.NUMERIC_SCALE AS Scale,
                    c.IS_NULLABLE AS IsNullable,
                    COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity') AS IsIdentity,
                    CASE WHEN pk.CONSTRAINT_TYPE = 'PRIMARY KEY' THEN 1 ELSE 0 END AS IsPrimaryKey
                FROM 
                    INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN (
                    SELECT 
                        k.TABLE_SCHEMA, 
                        k.TABLE_NAME, 
                        k.COLUMN_NAME, 
                        t.CONSTRAINT_TYPE
                    FROM 
                        INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
                    INNER JOIN 
                        INFORMATION_SCHEMA.TABLE_CONSTRAINTS t 
                    ON 
                        k.CONSTRAINT_NAME = t.CONSTRAINT_NAME AND 
                        k.TABLE_SCHEMA = t.TABLE_SCHEMA AND 
                        k.TABLE_NAME = t.TABLE_NAME
                    WHERE 
                        t.CONSTRAINT_TYPE = 'PRIMARY KEY'
                ) pk ON 
                    c.TABLE_SCHEMA = pk.TABLE_SCHEMA AND 
                    c.TABLE_NAME = pk.TABLE_NAME AND 
                    c.COLUMN_NAME = pk.COLUMN_NAME
                WHERE 
                    c.TABLE_NAME <> '{_options.HistoryTableName}'
                ORDER BY 
                    c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION
            ";
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                
                if (command.Connection.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();
                    
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        columns.Add(new ColumnDefinition
                        {
                            Name = reader["ColumnName"].ToString(),
                            DataType = reader["DataType"].ToString(),
                            MaxLength = reader["MaxLength"] != DBNull.Value ? Convert.ToInt32(reader["MaxLength"]) : (int?)null,
                            Precision = reader["Precision"] != DBNull.Value ? Convert.ToInt32(reader["Precision"]) : (int?)null,
                            Scale = reader["Scale"] != DBNull.Value ? Convert.ToInt32(reader["Scale"]) : (int?)null,
                            IsNullable = reader["IsNullable"].ToString() == "YES",
                            IsIdentity = Convert.ToInt32(reader["IsIdentity"]) == 1,
                            IsPrimaryKey = Convert.ToInt32(reader["IsPrimaryKey"]) == 1
                        });
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
            // 简化版类型兼容性检查
            string sqlType = property.GetSqlType(DatabaseProviderType.SqlServer).ToUpperInvariant();
            string columnType = column.DataType.ToUpperInvariant();
            
            // 对于字符串类型，检查长度变化
            if (sqlType.Contains("VARCHAR") || sqlType.Contains("NVARCHAR"))
            {
                if (columnType.Contains("VARCHAR") || columnType.Contains("NVARCHAR"))
                {
                    // 类型兼容，检查长度
                    if (property.MaxLength.HasValue && column.MaxLength.HasValue)
                    {
                        // 仅当需要增加长度时才需要修改
                        return property.MaxLength <= column.MaxLength;
                    }
                    
                    // MAX长度或其他情况，认为兼容
                    return true;
                }
                return false;
            }
            
            // 对于数值类型，检查精度和小数位数
            if (sqlType.Contains("DECIMAL") || sqlType.Contains("NUMERIC"))
            {
                if (columnType.Contains("DECIMAL") || columnType.Contains("NUMERIC"))
                {
                    // 类型兼容，检查精度和小数位数
                    if (property.Precision.HasValue && property.Scale.HasValue && 
                        column.Precision.HasValue && column.Scale.HasValue)
                    {
                        // 仅当需要增加精度或小数位数时才需要修改
                        return property.Precision <= column.Precision && property.Scale <= column.Scale;
                    }
                    
                    // 其他情况，认为兼容
                    return true;
                }
                return false;
            }
            
            // 对于其他类型，简单比较类型名称
            if (sqlType.Contains(columnType) || columnType.Contains(sqlType))
            {
                return true;
            }
            
            // 特殊情况处理
            if ((sqlType.Contains("INT") && columnType.Contains("INT")) ||
                (sqlType.Contains("DATETIME") && columnType.Contains("DATETIME")))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 生成创建表的SQL脚本
        /// </summary>
        protected override string GenerateCreateTableScript(EntityDefinition entity)
        {
            var sql = new StringBuilder();
            
            // 表名
            string fullTableName = string.IsNullOrEmpty(entity.Schema)
                ? $"[{entity.TableName}]"
                : $"[{entity.Schema}].[{entity.TableName}]";
                
            sql.AppendLine($"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{entity.TableName}'{(string.IsNullOrEmpty(entity.Schema) ? "" : $" AND TABLE_SCHEMA = '{entity.Schema}'")})");
            sql.AppendLine("BEGIN");
            sql.AppendLine($"    CREATE TABLE {fullTableName} (");
            
            // 列定义
            var columnDefinitions = new List<string>();
            var primaryKeys = new List<string>();
            
            foreach (var property in entity.Properties)
            {
                var columnDefinition = new StringBuilder();
                
                // 列名和类型
                columnDefinition.Append($"        [{property.ColumnName}] {property.GetSqlType(_providerType)}");
                
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
                    columnDefinition.Append(" IDENTITY(1,1)");
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
                var pkColumns = string.Join("], [", primaryKeys);
                columnDefinitions.Add($"        CONSTRAINT [PK_{entity.TableName}] PRIMARY KEY ([{pkColumns}])");
            }
            
            // 拼接列定义
            sql.AppendLine(string.Join(",\r\n", columnDefinitions));
            sql.AppendLine("    )");
            sql.AppendLine("END");
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成添加列的SQL脚本
        /// </summary>
        protected override string GenerateAddColumnScript(EntityDefinition entity, PropertyDefinition property)
        {
            var sql = new StringBuilder();
            
            // 表名
            string fullTableName = string.IsNullOrEmpty(entity.Schema)
                ? $"[{entity.TableName}]"
                : $"[{entity.Schema}].[{entity.TableName}]";
                
            // 列定义
            sql.Append($"ALTER TABLE {fullTableName} ADD [{property.ColumnName}] {property.GetSqlType(_providerType)}");
            
            // 可空性
            if (property.IsRequired)
            {
                sql.Append(" NOT NULL");
            }
            else
            {
                sql.Append(" NULL");
            }
            
            // 自增列需要额外处理，不能直接在ADD COLUMN中设置
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成修改列的SQL脚本
        /// </summary>
        protected override string GenerateAlterColumnScript(EntityDefinition entity, ColumnPropertyPair columnPair)
        {
            var sql = new StringBuilder();
            
            // 表名
            string fullTableName = string.IsNullOrEmpty(entity.Schema)
                ? $"[{entity.TableName}]"
                : $"[{entity.Schema}].[{entity.TableName}]";
                
            // 列定义
            sql.Append($"ALTER TABLE {fullTableName} ALTER COLUMN [{columnPair.Property.ColumnName}] {columnPair.Property.GetSqlType(_providerType)}");
            
            // 可空性
            if (columnPair.Property.IsRequired)
            {
                sql.Append(" NOT NULL");
            }
            else
            {
                sql.Append(" NULL");
            }
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成删除列的SQL脚本
        /// </summary>
        protected override string GenerateDropColumnScript(EntityDefinition entity, ColumnDefinition column)
        {
            var sql = new StringBuilder();
            
            // 表名
            string fullTableName = string.IsNullOrEmpty(entity.Schema)
                ? $"[{entity.TableName}]"
                : $"[{entity.Schema}].[{entity.TableName}]";
                
            // 删除列
            sql.Append($"ALTER TABLE {fullTableName} DROP COLUMN [{column.Name}]");
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成删除表的SQL脚本
        /// </summary>
        protected override string GenerateDropTableScript(TableDefinition table)
        {
            var sql = new StringBuilder();
            
            // 表名
            string fullTableName = string.IsNullOrEmpty(table.Schema)
                ? $"[{table.Name}]"
                : $"[{table.Schema}].[{table.Name}]";
                
            // 删除表
            sql.Append($"DROP TABLE {fullTableName}");
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 转义标识符
        /// </summary>
        protected override string EscapeIdentifier(string identifier)
        {
            return $"[{identifier}]";
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
            return new SqlParameter($"@{name}", value ?? DBNull.Value);
        }
        
        /// <summary>
        /// 获取当前日期时间函数
        /// </summary>
        protected override string GetCurrentDateTimeFunction()
        {
            return "GETUTCDATE()";
        }
    }
} 