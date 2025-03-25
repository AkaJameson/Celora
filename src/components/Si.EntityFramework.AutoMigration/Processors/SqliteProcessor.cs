using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.AutoMigration.Models;

namespace Si.EntityFramework.AutoMigration.Processors
{
    /// <summary>
    /// SQLite 处理器
    /// </summary>
    internal class SqliteProcessor : BaseDatabaseProcessor
    {
        public SqliteProcessor(DbContext context, AutoMigrationOptions options)
            : base(context, options, DatabaseProviderType.Sqlite)
        {
        }
        
        /// <summary>
        /// 确保迁移历史表存在
        /// </summary>
        public override async Task EnsureMigrationHistoryTableExistsAsync()
        {
            string sql = $@"
                CREATE TABLE IF NOT EXISTS {EscapeIdentifier(_options.HistoryTableName)} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TableName TEXT NOT NULL,
                    Operation TEXT NOT NULL,
                    SqlScript TEXT NULL,
                    AppliedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    Description TEXT NULL
                );
            ";
            
            await _context.Database.ExecuteSqlRawAsync(sql);
        }
        
        /// <summary>
        /// 获取数据库定义
        /// </summary>
        public override async Task<List<TableDefinition>> GetDatabaseDefinitionsAsync()
        {
            // 在执行迁移前备份数据库
            if (_options.BackupDatabase)
            {
                await BackupDatabaseAsync();
            }
            
            // 获取所有表（排除系统表和迁移历史表）
            var tables = await GetTablesAsync();
            
            // 获取所有表的结构信息
            foreach (var table in tables)
            {
                var columns = await GetColumnsForTableAsync(table.Name);
                table.Columns = columns;
            }
            
            return tables;
        }
        
        /// <summary>
        /// 备份数据库
        /// </summary>
        private async Task BackupDatabaseAsync()
        {
            try
            {
                // 获取数据库文件路径
                var connection = _context.Database.GetDbConnection() as SqliteConnection;
                var dataSource = connection.DataSource;
                
                if (string.IsNullOrEmpty(dataSource) || dataSource == ":memory:")
                {
                    // 内存数据库不需要备份
                    return;
                }
                
                // 创建备份文件
                string backupFile = $"{dataSource}.{DateTime.Now:yyyyMMddHHmmss}.bak";
                
                // 确保连接已打开
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                
                // 执行备份
                using (var backupConn = new SqliteConnection($"Data Source={backupFile}"))
                {
                    await backupConn.OpenAsync();
                    connection.BackupDatabase(backupConn);
                }
                
                Console.WriteLine($"数据库已备份到 {backupFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"备份数据库失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取所有表
        /// </summary>
        private async Task<List<TableDefinition>> GetTablesAsync()
        {
            var tables = new List<TableDefinition>();
            
            string sql = $@"
                SELECT name 
                FROM sqlite_master 
                WHERE type='table' 
                AND name NOT LIKE 'sqlite_%' 
                AND name <> '{_options.HistoryTableName}'
                ORDER BY name
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
                            Schema = "", // SQLite 不支持 Schema
                            Columns = new List<ColumnDefinition>()
                        });
                    }
                }
            }
            
            return tables;
        }
        
        /// <summary>
        /// 获取表的列信息
        /// </summary>
        private async Task<List<ColumnDefinition>> GetColumnsForTableAsync(string tableName)
        {
            var columns = new List<ColumnDefinition>();
            
            string sql = $"PRAGMA table_info({EscapeIdentifier(tableName)})";
            
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
                            Name = reader.GetString(1), // 列名
                            DataType = reader.GetString(2), // 数据类型
                            IsNullable = reader.GetInt32(3) == 0, // notnull 标志
                            IsPrimaryKey = reader.GetInt32(5) > 0, // pk 标志
                            IsIdentity = false // SQLite 无法通过 PRAGMA 直接检查是否为自增列
                        };
                        
                        // 尝试解析 MaxLength（如果有）
                        string dataType = column.DataType.ToUpperInvariant();
                        if (dataType.Contains("VARCHAR") || dataType.Contains("CHAR"))
                        {
                            int startIndex = dataType.IndexOf('(');
                            int endIndex = dataType.IndexOf(')');
                            
                            if (startIndex > 0 && endIndex > startIndex)
                            {
                                string lengthText = dataType.Substring(startIndex + 1, endIndex - startIndex - 1);
                                if (int.TryParse(lengthText, out int length))
                                {
                                    column.MaxLength = length;
                                }
                            }
                        }
                        
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
            // SQLite 类型系统非常宽松，基本上任何类型都可以存储任何值
            // 但我们仍然进行一些基本的类型检查
            
            string requiredType = property.GetSqlType(_providerType).ToUpperInvariant();
            string actualType = column.DataType.ToUpperInvariant();
            
            // 整数类型兼容检查
            if (requiredType == "INTEGER" && 
                (actualType == "INTEGER" || actualType == "INT" || actualType.Contains("INT")))
            {
                return true;
            }
            
            // 浮点类型兼容检查
            if (requiredType == "REAL" &&
                (actualType == "REAL" || actualType == "FLOAT" || actualType == "DOUBLE" || 
                 actualType == "DECIMAL" || actualType == "NUMERIC"))
            {
                return true;
            }
            
            // 文本类型兼容检查
            if (requiredType == "TEXT" &&
                (actualType == "TEXT" || actualType == "CHAR" || actualType.Contains("CHAR") || 
                 actualType == "CLOB" || actualType.Contains("VARCHAR")))
            {
                return true;
            }
            
            // 二进制类型兼容检查
            if (requiredType == "BLOB" &&
                (actualType == "BLOB" || actualType == "BINARY" || actualType.Contains("BINARY") ||
                 actualType.Contains("VARBINARY")))
            {
                return true;
            }
            
            // 日期类型在 SQLite 中通常以 TEXT 存储
            if ((requiredType == "TEXT" || requiredType == "DATETIME") &&
                (actualType == "TEXT" || actualType == "DATE" || actualType == "DATETIME" || 
                 actualType == "TIMESTAMP"))
            {
                return true;
            }
            
            // 在 SQLite 中，如果类型名称相同或者为 SQLite 支持的仿射类型，则认为兼容
            if (requiredType == actualType || 
                actualType == "INTEGER" || actualType == "REAL" || 
                actualType == "TEXT" || actualType == "BLOB" || actualType == "NUMERIC")
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
                
                // 主键
                if (property.IsPrimaryKey)
                {
                    primaryKeys.Add(property.ColumnName);
                }
                
                // 自增 - SQLite 只支持主键的 AUTOINCREMENT
                if (property.IsIdentity && property.IsPrimaryKey && primaryKeys.Count == 1)
                {
                    columnDefinition.Append(" PRIMARY KEY AUTOINCREMENT");
                    primaryKeys.Clear(); // 清空主键列表，因为已经在列定义中添加了 PRIMARY KEY
                }
                
                columnDefinitions.Add(columnDefinition.ToString());
            }
            
            // 添加主键约束（如果有多个主键列或者主键列不是自增的）
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
            // SQLite 不支持在添加列时设置主键或外键约束
            var sql = new StringBuilder();
            
            sql.Append($"ALTER TABLE {EscapeIdentifier(entity.TableName)} ADD COLUMN {EscapeIdentifier(property.ColumnName)} {property.GetSqlType(_providerType)}");
            
            // 可空性
            if (property.IsRequired)
            {
                sql.Append(" NOT NULL");
                
                // SQLite 在添加非空列时必须指定默认值
                sql.Append(" DEFAULT ''");
            }
            
            return sql.ToString();
        }
        
        /// <summary>
        /// 生成修改列的SQL脚本
        /// </summary>
        protected override string GenerateAlterColumnScript(EntityDefinition entity, ColumnPropertyPair columnPair)
        {
            // SQLite 不直接支持 ALTER COLUMN
            // 需要创建临时表、复制数据、删除原表、重命名临时表
            // 这是一个复杂操作，可能需要多个 SQL 语句，并且需要处理多种边缘情况
            
            // 简化实现：提示用户 SQLite 不支持直接修改列
            throw new NotSupportedException(
                "SQLite 不支持直接修改列。需要手动创建临时表、复制数据、删除原表并重命名临时表。" +
                "请考虑使用 SQLite Expert 或其他工具来修改表结构。"
            );
        }
        
        /// <summary>
        /// 生成删除列的SQL脚本
        /// </summary>
        protected override string GenerateDropColumnScript(EntityDefinition entity, ColumnDefinition column)
        {
            // SQLite 3.35.0（2021年3月）及更高版本支持 DROP COLUMN
            string sqliteVersion = GetSqliteVersion();
            if (CompareVersions(sqliteVersion, "3.35.0") >= 0)
            {
                return $"ALTER TABLE {EscapeIdentifier(entity.TableName)} DROP COLUMN {EscapeIdentifier(column.Name)}";
            }
            
            // 早期版本不支持，需要使用与 ALTER COLUMN 相同的替代方案
            throw new NotSupportedException(
                $"当前 SQLite 版本 {sqliteVersion} 不支持直接删除列。需要 SQLite 3.35.0 或更高版本。"
            );
        }
        
        /// <summary>
        /// 获取 SQLite 版本
        /// </summary>
        private string GetSqliteVersion()
        {
            try
            {
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "SELECT sqlite_version()";
                    
                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }
                    
                    return cmd.ExecuteScalar().ToString();
                }
            }
            catch
            {
                // 如果无法获取版本，假设使用的是较低版本
                return "0.0.0";
            }
        }
        
        /// <summary>
        /// 比较版本号
        /// </summary>
        private int CompareVersions(string version1, string version2)
        {
            var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var v2Parts = version2.Split('.').Select(int.Parse).ToArray();
            
            int length = Math.Max(v1Parts.Length, v2Parts.Length);
            
            for (int i = 0; i < length; i++)
            {
                int v1 = i < v1Parts.Length ? v1Parts[i] : 0;
                int v2 = i < v2Parts.Length ? v2Parts[i] : 0;
                
                if (v1 < v2) return -1;
                if (v1 > v2) return 1;
            }
            
            return 0;
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
            // SQLite 使用双引号或方括号转义标识符
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
            return new SqliteParameter($"@{name}", value ?? DBNull.Value);
        }
        
        /// <summary>
        /// 获取当前日期时间函数
        /// </summary>
        protected override string GetCurrentDateTimeFunction()
        {
            return "datetime('now')";
        }
    }
} 