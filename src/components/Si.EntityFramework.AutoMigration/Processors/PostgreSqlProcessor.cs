using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Si.EntityFramework.AutoMigration.Models;

namespace Si.EntityFramework.AutoMigration.Processors
{
    /// <summary>
    /// PostgreSQL 处理器
    /// </summary>
    internal class PostgreSqlProcessor : BaseDatabaseProcessor
    {
        public PostgreSqlProcessor(DbContext context, AutoMigrationOptions options)
            : base(context, options, DatabaseProviderType.PostgreSql)
        {
        }
        
        /// <summary>
        /// 确保迁移历史表存在
        /// </summary>
        public override async Task EnsureMigrationHistoryTableExistsAsync()
        {
            string sql = $@"
                CREATE TABLE IF NOT EXISTS {EscapeIdentifier(_options.HistoryTableName)} (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""TableName"" VARCHAR(128) NOT NULL,
                    ""Operation"" VARCHAR(50) NOT NULL,
                    ""SqlScript"" TEXT NULL,
                    ""AppliedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""Description"" VARCHAR(500) NULL
                );
            ";
            
            await _context.Database.ExecuteSqlRawAsync(sql);
        }
        
        /// <summary>
        /// 获取数据库定义
        /// </summary>
        public override async Task<List<TableDefinition>> GetDatabaseDefinitionsAsync()
        {
            // 获取当前数据库模式名称
            string schemaName = await GetCurrentSchemaAsync();
            
            // 获取所有表
            var tables = await GetTablesAsync(schemaName);
            
            // 获取所有列
            foreach (var table in tables)
            {
                var columns = await GetColumnsForTableAsync(schemaName, table.Name);
                table.Columns = columns;
            }
            
            return tables;
        }
        
        /// <summary>
        /// 获取当前模式名称
        /// </summary>
        private async Task<string> GetCurrentSchemaAsync()
        {
            string sql = "SELECT current_schema()";
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                
                if (command.Connection.State != ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                
                var result = await command.ExecuteScalarAsync();
                return result?.ToString() ?? "public";
            }
        }
        
        /// <summary>
        /// 获取所有表
        /// </summary>
        private async Task<List<TableDefinition>> GetTablesAsync(string schemaName)
        {
            var tables = new List<TableDefinition>();
            
            string sql = $@"
                SELECT table_name
                FROM information_schema.tables
                WHERE table_schema = '{schemaName}'
                AND table_type = 'BASE TABLE'
                AND table_name <> '{_options.HistoryTableName}'
                ORDER BY table_name
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
                            Schema = schemaName,
                            Columns = new List<ColumnDefinition>()
                        });
                    }
                }
            }
            
            return tables;
        }
        
        /// <summary>
        /// 获取表的列
        /// </summary>
        private async Task<List<ColumnDefinition>> GetColumnsForTableAsync(string schemaName, string tableName)
        {
            var columns = new List<ColumnDefinition>();
            
            string sql = $@"
                SELECT 
                    column_name,
                    data_type,
                    character_maximum_length,
                    numeric_precision,
                    numeric_scale,
                    is_nullable,
                    column_default,
                    pg_get_serial_sequence('{schemaName}.{tableName}', column_name) IS NOT NULL AS is_identity
                FROM 
                    information_schema.columns
                WHERE 
                    table_schema = '{schemaName}'
                    AND table_name = '{tableName}'
                ORDER BY
                    ordinal_position;
            ";
            
            string pkSql = $@"
                SELECT 
                    a.attname
                FROM 
                    pg_index i
                JOIN 
                    pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
                WHERE 
                    i.indrelid = '{schemaName}.{tableName}'::regclass
                    AND i.indisprimary;
            ";
            
            // 获取主键列
            var primaryKeys = new List<string>();
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = pkSql;
                
                if (command.Connection.State != ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        primaryKeys.Add(reader.GetString(0));
                    }
                }
            }
            
            // 获取列信息
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
                        string columnName = reader.GetString(0);
                        
                        var column = new ColumnDefinition
                        {
                            Name = columnName,
                            DataType = reader.GetString(1),
                            MaxLength = !reader.IsDBNull(2) ? reader.GetInt32(2) : (int?)null,
                            Precision = !reader.IsDBNull(3) ? reader.GetInt32(3) : (int?)null,
                            Scale = !reader.IsDBNull(4) ? reader.GetInt32(4) : (int?)null,
                            IsNullable = reader.GetString(5) == "YES",
                            IsIdentity = !reader.IsDBNull(7) && reader.GetBoolean(7),
                            IsPrimaryKey = primaryKeys.Contains(columnName)
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
            string propertyType = property.GetSqlType(_providerType).ToLowerInvariant();
            string columnType = column.DataType.ToLowerInvariant();
            
            // 整数类型兼容检查
            bool isPropertyIntegerType = IsIntegerType(propertyType);
            bool isColumnIntegerType = IsIntegerType(columnType);
            
            if (isPropertyIntegerType && isColumnIntegerType)
            {
                // 检查整数类型大小
                int propertySize = GetIntegerTypeSize(propertyType);
                int columnSize = GetIntegerTypeSize(columnType);
                
                // 如果需要更大的整数类型，认为不兼容
                if (propertySize > columnSize)
                {
                    return false;
                }
                
                return true;
            }
            
            // 字符串类型兼容检查
            bool isPropertyStringType = IsStringType(propertyType);
            bool isColumnStringType = IsStringType(columnType);
            
            if (isPropertyStringType && isColumnStringType)
            {
                // 检查长度是否兼容
                if (property.MaxLength.HasValue && column.MaxLength.HasValue)
                {
                    // 如果需要更长的字符串长度，认为不兼容
                    if (property.MaxLength > column.MaxLength)
                    {
                        return false;
                    }
                }
                
                return true;
            }
            
            // 浮点类型兼容检查
            bool isPropertyFloatType = IsFloatType(propertyType);
            bool isColumnFloatType = IsFloatType(columnType);
            
            if (isPropertyFloatType && isColumnFloatType)
            {
                // 检查精度和小数位数是否兼容
                if (property.Precision.HasValue && property.Scale.HasValue && 
                    column.Precision.HasValue && column.Scale.HasValue)
                {
                    // 如果需要更高的精度或小数位数，认为不兼容
                    if (property.Precision > column.Precision || property.Scale > column.Scale)
                    {
                        return false;
                    }
                }
                
                return true;
            }
            
            // 日期时间类型兼容检查
            bool isPropertyDateTimeType = IsDateTimeType(propertyType);
            bool isColumnDateTimeType = IsDateTimeType(columnType);
            
            if (isPropertyDateTimeType && isColumnDateTimeType)
            {
                // timestamp, timestamptz, date, time 之间不一定兼容
                return propertyType == columnType;
            }
            
            // 二进制类型兼容检查
            bool isPropertyBinaryType = IsBinaryType(propertyType);
            bool isColumnBinaryType = IsBinaryType(columnType);
            
            if (isPropertyBinaryType && isColumnBinaryType)
            {
                return true;
            }
            
            // 布尔类型兼容检查
            if (propertyType == "boolean" && columnType == "boolean")
            {
                return true;
            }
            
            // UUID类型兼容检查
            if (propertyType == "uuid" && columnType == "uuid")
            {
                return true;
            }
            
            // 其他类型，只有完全相同才兼容
            return propertyType == columnType;
        }
        
        /// <summary>
        /// 是否为整数类型
        /// </summary>
        private bool IsIntegerType(string type)
        {
            return type == "smallint" || type == "integer" || type == "bigint" || 
                   type == "smallserial" || type == "serial" || type == "bigserial";
        }
        
        /// <summary>
        /// 获取整数类型大小
        /// </summary>
        private int GetIntegerTypeSize(string type)
        {
            if (type == "smallint" || type == "smallserial") return 2;
            if (type == "integer" || type == "serial") return 4;
            if (type == "bigint" || type == "bigserial") return 8;
            return 0;
        }
        
        /// <summary>
        /// 是否为字符串类型
        /// </summary>
        private bool IsStringType(string type)
        {
            return type.Contains("char") || type == "text";
        }
        
        /// <summary>
        /// 是否为浮点类型
        /// </summary>
        private bool IsFloatType(string type)
        {
            return type == "real" || type == "double precision" || type.Contains("decimal") || 
                   type.Contains("numeric");
        }
        
        /// <summary>
        /// 是否为日期时间类型
        /// </summary>
        private bool IsDateTimeType(string type)
        {
            return type.Contains("timestamp") || type == "date" || type == "time";
        }
        
        /// <summary>
        /// 是否为二进制类型
        /// </summary>
        private bool IsBinaryType(string type)
        {
            return type == "bytea";
        }
        
        /// <summary>
        /// 生成创建表的SQL脚本
        /// </summary>
        protected override string GenerateCreateTableScript(EntityDefinition entity)
        {
            var sql = new StringBuilder();
            
            string fullTableName = string.IsNullOrEmpty(entity.Schema)
                ? EscapeIdentifier(entity.TableName)
                : $"{EscapeIdentifier(entity.Schema)}.{EscapeIdentifier(entity.TableName)}";
                
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS {fullTableName} (");
            
            // 列定义
            var columnDefinitions = new List<string>();
            var primaryKeys = new List<string>();
            
            foreach (var property in entity.Properties)
            {
                var columnDefinition = new StringBuilder();
                
                // 列名和类型
                string dataType = property.GetSqlType(_providerType);
                
                // 如果是自增列，使用SERIAL或BIGSERIAL
                if (property.IsIdentity)
                {
                    if (property.ClrType == typeof(int))
                    {
                        dataType = "SERIAL";
                    }
                    else if (property.ClrType == typeof(long))
                    {
                        dataType = "BIGSERIAL";
                    }
                }
                
                columnDefinition.Append($"    {EscapeIdentifier(property.ColumnName)} {dataType}");
                
                // 可空性
                if (property.IsRequired)
                {
                    columnDefinition.Append(" NOT NULL");
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
            sql.AppendLine(");");
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成添加列的SQL脚本
        /// </summary>
        protected override string GenerateAddColumnScript(EntityDefinition entity, PropertyDefinition property)
        {
            string fullTableName = string.IsNullOrEmpty(entity.Schema)
                ? EscapeIdentifier(entity.TableName)
                : $"{EscapeIdentifier(entity.Schema)}.{EscapeIdentifier(entity.TableName)}";
                
            var sql = new StringBuilder();
            
            // 列名和类型
            string dataType = property.GetSqlType(_providerType);
            
            // 如果是自增列，使用独立语句设置序列
            if (property.IsIdentity)
            {
                if (property.ClrType == typeof(int))
                {
                    sql.AppendLine($"ALTER TABLE {fullTableName} ADD COLUMN {EscapeIdentifier(property.ColumnName)} INTEGER");
                    sql.AppendLine($"CREATE SEQUENCE IF NOT EXISTS {entity.TableName}_{property.ColumnName}_seq OWNED BY {fullTableName}.{EscapeIdentifier(property.ColumnName)};");
                    sql.AppendLine($"ALTER TABLE {fullTableName} ALTER COLUMN {EscapeIdentifier(property.ColumnName)} SET DEFAULT nextval('{entity.TableName}_{property.ColumnName}_seq');");
                }
                else if (property.ClrType == typeof(long))
                {
                    sql.AppendLine($"ALTER TABLE {fullTableName} ADD COLUMN {EscapeIdentifier(property.ColumnName)} BIGINT");
                    sql.AppendLine($"CREATE SEQUENCE IF NOT EXISTS {entity.TableName}_{property.ColumnName}_seq OWNED BY {fullTableName}.{EscapeIdentifier(property.ColumnName)};");
                    sql.AppendLine($"ALTER TABLE {fullTableName} ALTER COLUMN {EscapeIdentifier(property.ColumnName)} SET DEFAULT nextval('{entity.TableName}_{property.ColumnName}_seq');");
                }
            }
            else
            {
                sql.Append($"ALTER TABLE {fullTableName} ADD COLUMN {EscapeIdentifier(property.ColumnName)} {dataType}");
                
                // 可空性
                if (property.IsRequired)
                {
                    sql.Append(" NOT NULL");
                }
            }
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成修改列的SQL脚本
        /// </summary>
        protected override string GenerateAlterColumnScript(EntityDefinition entity, ColumnPropertyPair columnPair)
        {
            string fullTableName = string.IsNullOrEmpty(entity.Schema)
                ? EscapeIdentifier(entity.TableName)
                : $"{EscapeIdentifier(entity.Schema)}.{EscapeIdentifier(entity.TableName)}";
                
            var sql = new StringBuilder();
            
            // 修改列类型
            string dataType = columnPair.Property.GetSqlType(_providerType);
            sql.AppendLine($"ALTER TABLE {fullTableName} ALTER COLUMN {EscapeIdentifier(columnPair.Property.ColumnName)} TYPE {dataType} USING {EscapeIdentifier(columnPair.Property.ColumnName)}::{dataType};");
            
            // 修改可空性
            if (columnPair.Property.IsRequired)
            {
                sql.AppendLine($"ALTER TABLE {fullTableName} ALTER COLUMN {EscapeIdentifier(columnPair.Property.ColumnName)} SET NOT NULL;");
            }
            else
            {
                sql.AppendLine($"ALTER TABLE {fullTableName} ALTER COLUMN {EscapeIdentifier(columnPair.Property.ColumnName)} DROP NOT NULL;");
            }
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成删除列的SQL脚本
        /// </summary>
        protected override string GenerateDropColumnScript(EntityDefinition entity, ColumnDefinition column)
        {
            string fullTableName = string.IsNullOrEmpty(entity.Schema)
                ? EscapeIdentifier(entity.TableName)
                : $"{EscapeIdentifier(entity.Schema)}.{EscapeIdentifier(entity.TableName)}";
                
            return $"ALTER TABLE {fullTableName} DROP COLUMN IF EXISTS {EscapeIdentifier(column.Name)}";
        }
        
        /// <summary>
        /// 生成删除表的SQL脚本
        /// </summary>
        protected override string GenerateDropTableScript(TableDefinition table)
        {
            string fullTableName = string.IsNullOrEmpty(table.Schema)
                ? EscapeIdentifier(table.Name)
                : $"{EscapeIdentifier(table.Schema)}.{EscapeIdentifier(table.Name)}";
                
            return $"DROP TABLE IF EXISTS {fullTableName}";
        }
        
        /// <summary>
        /// 转义标识符
        /// </summary>
        protected override string EscapeIdentifier(string identifier)
        {
            // PostgreSQL 使用双引号转义标识符
            return $"\"{identifier.Replace("\"", "\"\"")}\"";
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
            return new NpgsqlParameter($"@{name}", value ?? DBNull.Value);
        }
        
        /// <summary>
        /// 获取当前日期时间函数
        /// </summary>
        protected override string GetCurrentDateTimeFunction()
        {
            return "CURRENT_TIMESTAMP";
        }
    }
} 