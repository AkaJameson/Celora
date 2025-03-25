using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 基础表定义
    /// </summary>
    public abstract class BaseTableDefinition : ITableDefinition
    {
        private readonly List<TableColumnDefinition> _columns = new List<TableColumnDefinition>();
        private readonly List<TableIndexDefinition> _indexes = new List<TableIndexDefinition>();
        private TableColumnDefinition _primaryKey;

        /// <summary>
        /// 获取基础表名
        /// </summary>
        public abstract string BaseTableName { get; }

        /// <summary>
        /// 获取表字段定义
        /// </summary>
        public IEnumerable<TableColumnDefinition> GetColumns() => _columns;

        /// <summary>
        /// 获取主键定义
        /// </summary>
        public TableColumnDefinition GetPrimaryKey() => _primaryKey;

        /// <summary>
        /// 获取索引定义
        /// </summary>
        public IEnumerable<TableIndexDefinition> GetIndexes() => _indexes;

        /// <summary>
        /// 添加字段
        /// </summary>
        protected void AddColumn(TableColumnDefinition column)
        {
            _columns.Add(column);
        }

        /// <summary>
        /// 设置主键
        /// </summary>
        protected void SetPrimaryKey(TableColumnDefinition primaryKey)
        {
            _primaryKey = primaryKey;
            if (!_columns.Contains(primaryKey))
            {
                _columns.Add(primaryKey);
            }
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        protected void AddIndex(TableIndexDefinition index)
        {
            _indexes.Add(index);
        }

        /// <summary>
        /// 根据数据库类型生成建表SQL
        /// </summary>
        public virtual string GenerateCreateTableSql(string tableName, DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.SQLite:
                    return GenerateSQLiteCreateTableSql(tableName);
                case DatabaseType.MySQL:
                    return GenerateMySqlCreateTableSql(tableName);
                case DatabaseType.PostgreSQL:
                    return GeneratePostgreSqlCreateTableSql(tableName);
                case DatabaseType.SQLServer:
                    return GenerateSqlServerCreateTableSql(tableName);
                default:
                    throw new System.NotSupportedException($"不支持的数据库类型：{dbType}");
            }
        }

        private string GenerateSQLiteCreateTableSql(string tableName)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

            var columnDefinitions = new List<string>();
            foreach (var column in _columns)
            {
                var columnDef = $"    {column.Name} {column.TypeMapping[DatabaseType.SQLite]}";
                
                if (column == _primaryKey)
                {
                    columnDef += " PRIMARY KEY";
                    if (column.IsAutoIncrement)
                    {
                        columnDef += " AUTOINCREMENT";
                    }
                }
                
                if (!column.AllowNull && column != _primaryKey)
                {
                    columnDef += " NOT NULL";
                }
                
                if (column.DefaultValue != null)
                {
                    columnDef += $" DEFAULT {column.DefaultValue}";
                }
                
                columnDefinitions.Add(columnDef);
            }

            sql.AppendLine(string.Join(",\n", columnDefinitions));
            sql.AppendLine(");");

            // 添加索引
            foreach (var index in _indexes)
            {
                var indexType = index.IsUnique ? "UNIQUE INDEX" : "INDEX";
                sql.AppendLine($"CREATE {indexType} IF NOT EXISTS {index.Name} ON {tableName} ({string.Join(", ", index.Columns)});");
            }

            return sql.ToString();
        }

        private string GenerateMySqlCreateTableSql(string tableName)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

            var columnDefinitions = new List<string>();
            foreach (var column in _columns)
            {
                var columnDef = $"    {column.Name} {column.TypeMapping[DatabaseType.MySQL]}";
                
                if (!column.AllowNull)
                {
                    columnDef += " NOT NULL";
                }
                
                if (column.DefaultValue != null)
                {
                    columnDef += $" DEFAULT {column.DefaultValue}";
                }
                
                if (column.IsAutoIncrement && column == _primaryKey)
                {
                    columnDef += " AUTO_INCREMENT";
                }
                
                if (column.Comment != null)
                {
                    columnDef += $" COMMENT '{column.Comment}'";
                }
                
                columnDefinitions.Add(columnDef);
            }

            if (_primaryKey != null)
            {
                columnDefinitions.Add($"    PRIMARY KEY ({_primaryKey.Name})");
            }

            // 添加索引
            foreach (var index in _indexes)
            {
                var indexType = index.IsUnique ? "UNIQUE KEY" : "KEY";
                columnDefinitions.Add($"    {indexType} {index.Name} ({string.Join(", ", index.Columns)})");
            }

            sql.AppendLine(string.Join(",\n", columnDefinitions));
            sql.AppendLine(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            return sql.ToString();
        }

        private string GeneratePostgreSqlCreateTableSql(string tableName)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

            var columnDefinitions = new List<string>();
            foreach (var column in _columns)
            {
                var columnDef = $"    {column.Name} {column.TypeMapping[DatabaseType.PostgreSQL]}";
                
                if (column == _primaryKey)
                {
                    columnDef += " PRIMARY KEY";
                }
                
                if (!column.AllowNull && column != _primaryKey)
                {
                    columnDef += " NOT NULL";
                }
                
                if (column.DefaultValue != null)
                {
                    columnDef += $" DEFAULT {column.DefaultValue}";
                }
                
                columnDefinitions.Add(columnDef);
            }

            sql.AppendLine(string.Join(",\n", columnDefinitions));
            sql.AppendLine(");");

            // 如果主键是自增的，为PostgreSQL添加序列
            if (_primaryKey != null && _primaryKey.IsAutoIncrement)
            {
                var sequenceName = $"{tableName}_{_primaryKey.Name}_seq";
                sql.AppendLine($"CREATE SEQUENCE IF NOT EXISTS {sequenceName} OWNED BY {tableName}.{_primaryKey.Name};");
                sql.AppendLine($"ALTER TABLE {tableName} ALTER COLUMN {_primaryKey.Name} SET DEFAULT nextval('{sequenceName}');");
            }

            // 添加索引
            foreach (var index in _indexes)
            {
                var indexType = index.IsUnique ? "UNIQUE INDEX" : "INDEX";
                sql.AppendLine($"CREATE {indexType} IF NOT EXISTS {index.Name} ON {tableName} ({string.Join(", ", index.Columns)});");
            }

            // 添加注释
            foreach (var column in _columns.Where(c => c.Comment != null))
            {
                sql.AppendLine($"COMMENT ON COLUMN {tableName}.{column.Name} IS '{column.Comment}';");
            }

            return sql.ToString();
        }

        private string GenerateSqlServerCreateTableSql(string tableName)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{tableName}]') AND type in (N'U'))");
            sql.AppendLine("BEGIN");
            sql.AppendLine($"CREATE TABLE [dbo].[{tableName}] (");

            var columnDefinitions = new List<string>();
            foreach (var column in _columns)
            {
                var columnDef = $"    [{column.Name}] {column.TypeMapping[DatabaseType.SQLServer]}";
                
                if (column.IsAutoIncrement && column == _primaryKey)
                {
                    columnDef += " IDENTITY(1,1)";
                }
                
                if (!column.AllowNull)
                {
                    columnDef += " NOT NULL";
                }
                
                if (column.DefaultValue != null)
                {
                    columnDef += $" DEFAULT ({column.DefaultValue})";
                }
                
                columnDefinitions.Add(columnDef);
            }

            if (_primaryKey != null)
            {
                columnDefinitions.Add($"    CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ([{_primaryKey.Name}])");
            }

            sql.AppendLine(string.Join(",\n", columnDefinitions));
            sql.AppendLine(")");
            sql.AppendLine("END");

            // 添加索引
            foreach (var index in _indexes)
            {
                var indexType = index.IsUnique ? "UNIQUE" : "";
                sql.AppendLine($"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = '{index.Name}' AND object_id = OBJECT_ID(N'[dbo].[{tableName}]'))");
                sql.AppendLine("BEGIN");
                sql.AppendLine($"CREATE {indexType} INDEX [{index.Name}] ON [dbo].[{tableName}] ({string.Join(", ", index.Columns.Select(c => $"[{c}]"))})");
                sql.AppendLine("END");
            }

            return sql.ToString();
        }
    }
} 