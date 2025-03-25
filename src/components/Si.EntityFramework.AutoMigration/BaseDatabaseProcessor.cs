using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Si.EntityFramework.AutoMigration.Models;

namespace Si.EntityFramework.AutoMigration
{
    /// <summary>
    /// 基础数据库处理器
    /// </summary>
    public abstract class BaseDatabaseProcessor : IDatabaseProcessor
    {
        protected readonly DbContext _context;
        protected readonly AutoMigrationOptions _options;
        protected readonly DatabaseProviderType _providerType;

        public BaseDatabaseProcessor(DbContext context, AutoMigrationOptions options, DatabaseProviderType providerType)
        {
            _context = context;
            _options = options;
            _providerType = providerType;
        }

        /// <summary>
        /// 确保迁移历史表存在
        /// </summary>
        public abstract Task EnsureMigrationHistoryTableExistsAsync();

        /// <summary>
        /// 获取数据库定义
        /// </summary>
        public abstract Task<List<TableDefinition>> GetDatabaseDefinitionsAsync();

        /// <summary>
        /// 获取模型定义
        /// </summary>
        public virtual List<EntityDefinition> GetModelDefinitions()
        {
            var entityDefinitions = new List<EntityDefinition>();
            var entityTypes = _context.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                // 跳过未映射到表的实体
                if (entityType.GetTableName() == null)
                    continue;

                // 跳过迁移历史表
                if (entityType.GetTableName().Equals(_options.HistoryTableName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var entityDefinition = new EntityDefinition
                {
                    ClrType = entityType.ClrType,
                    TableName = entityType.GetTableName(),
                    Schema = entityType.GetSchema(),
                    Properties = new List<PropertyDefinition>()
                };

                foreach (var property in entityType.GetProperties())
                {
                    // 跳过不映射到列的属性
                    if (property.GetColumnName() == null)
                        continue;

                    var propertyDefinition = new PropertyDefinition
                    {
                        PropertyInfo = property.PropertyInfo,
                        ColumnName = property.GetColumnName(),
                        ClrType = property.ClrType,
                        IsRequired = !property.IsNullable,
                        IsPrimaryKey = property.IsPrimaryKey(),
                        IsIdentity = property.ValueGenerated == ValueGenerated.OnAdd && (
                            property.ClrType == typeof(int) ||
                            property.ClrType == typeof(long)
                        ),
                        MaxLength = property.GetMaxLength(),
                        Precision = property.GetPrecision(),
                        Scale = property.GetScale()
                    };

                    entityDefinition.Properties.Add(propertyDefinition);
                }

                entityDefinitions.Add(entityDefinition);
            }

            return entityDefinitions;
        }

        /// <summary>
        /// 比较差异
        /// </summary>
        public virtual SchemaDifference CompareDifferences(List<EntityDefinition> modelDefinitions, List<TableDefinition> databaseDefinitions)
        {
            var differences = new SchemaDifference
            {
                TablesToCreate = new List<EntityDefinition>(),
                TablesToDelete = new List<TableDefinition>(),
                TableChanges = new List<TableChange>()
            };

            // 1. 查找需要创建的表
            foreach (var entity in modelDefinitions)
            {
                var existingTable = databaseDefinitions.FirstOrDefault(t =>
                    string.Equals(t.Name, entity.TableName, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrEmpty(entity.Schema) ||
                     string.Equals(t.Schema, entity.Schema, StringComparison.OrdinalIgnoreCase))
                );

                if (existingTable == null)
                {
                    differences.TablesToCreate.Add(entity);
                }
                else
                {
                    // 表存在，检查列差异
                    var tableChanges = new TableChange
                    {
                        Entity = entity,
                        Table = existingTable,
                        ColumnsToAdd = new List<PropertyDefinition>(),
                        ColumnsToAlter = new List<ColumnPropertyPair>(),
                        ColumnsToDelete = new List<ColumnDefinition>()
                    };

                    // 检查列差异
                    foreach (var property in entity.Properties)
                    {
                        var existingColumn = existingTable.Columns.FirstOrDefault(c =>
                            string.Equals(c.Name, property.ColumnName, StringComparison.OrdinalIgnoreCase)
                        );

                        if (existingColumn == null)
                        {
                            // 列不存在，需要添加
                            tableChanges.ColumnsToAdd.Add(property);
                        }
                        else
                        {
                            // 列存在，检查属性差异
                            bool needsChange = false;

                            // 检查数据类型差异
                            needsChange |= !IsCompatibleDataType(property, existingColumn);

                            // 检查可空性差异 - 仅当改为不可空时才需要修改
                            needsChange |= property.IsRequired && existingColumn.IsNullable;

                            // 检查最大长度差异 - 仅当需要增加长度时才需要修改
                            if (property.MaxLength.HasValue && existingColumn.MaxLength.HasValue)
                            {
                                needsChange |= property.MaxLength > existingColumn.MaxLength;
                            }

                            // 如果有差异，添加到修改列表
                            if (needsChange)
                            {
                                tableChanges.ColumnsToAlter.Add(new ColumnPropertyPair
                                {
                                    Property = property,
                                    Column = existingColumn
                                });
                            }
                        }
                    }

                    // 检查需要删除的列
                    if (_options.AllowDropColumn)
                    {
                        foreach (var column in existingTable.Columns)
                        {
                            var existingProperty = entity.Properties.FirstOrDefault(p =>
                                string.Equals(p.ColumnName, column.Name, StringComparison.OrdinalIgnoreCase)
                            );

                            if (existingProperty == null)
                            {
                                tableChanges.ColumnsToDelete.Add(column);
                            }
                        }
                    }

                    // 只有存在变更时才添加到列表
                    if (tableChanges.HasChanges)
                    {
                        differences.TableChanges.Add(tableChanges);
                    }
                }
            }

            // 2. 查找需要删除的表
            if (_options.AllowDropTable)
            {
                foreach (var table in databaseDefinitions)
                {
                    var existingEntity = modelDefinitions.FirstOrDefault(e =>
                        string.Equals(e.TableName, table.Name, StringComparison.OrdinalIgnoreCase) &&
                        (string.IsNullOrEmpty(e.Schema) ||
                         string.Equals(table.Schema, e.Schema, StringComparison.OrdinalIgnoreCase))
                    );

                    if (existingEntity == null)
                    {
                        differences.TablesToDelete.Add(table);
                    }
                }
            }

            return differences;
        }

        /// <summary>
        /// 检查数据类型兼容性
        /// </summary>
        protected abstract bool IsCompatibleDataType(PropertyDefinition property, ColumnDefinition column);

        /// <summary>
        /// 生成迁移脚本
        /// </summary>
        public virtual List<MigrationScript> GenerateMigrationScripts(SchemaDifference differences)
        {
            var scripts = new List<MigrationScript>();

            // 处理创建表
            foreach (var table in differences.TablesToCreate)
            {
                scripts.Add(new MigrationScript
                {
                    TableName = table.TableName,
                    Operation = "CreateTable",
                    Sql = GenerateCreateTableScript(table),
                    Description = $"创建表 {table.GetFullName()}"
                });
            }

            // 处理表修改
            foreach (var tableChange in differences.TableChanges)
            {
                // 处理添加列
                foreach (var column in tableChange.ColumnsToAdd)
                {
                    scripts.Add(new MigrationScript
                    {
                        TableName = tableChange.Entity.TableName,
                        Operation = "AddColumn",
                        Sql = GenerateAddColumnScript(tableChange.Entity, column),
                        Description = $"添加列 {column.ColumnName} 到表 {tableChange.Entity.GetFullName()}"
                    });
                }

                // 处理修改列
                foreach (var columnPair in tableChange.ColumnsToAlter)
                {
                    scripts.Add(new MigrationScript
                    {
                        TableName = tableChange.Entity.TableName,
                        Operation = "AlterColumn",
                        Sql = GenerateAlterColumnScript(tableChange.Entity, columnPair),
                        Description = $"修改列 {columnPair.Property.ColumnName} 在表 {tableChange.Entity.GetFullName()}"
                    });
                }

                // 处理删除列
                foreach (var column in tableChange.ColumnsToDelete)
                {
                    scripts.Add(new MigrationScript
                    {
                        TableName = tableChange.Entity.TableName,
                        Operation = "DropColumn",
                        Sql = GenerateDropColumnScript(tableChange.Entity, column),
                        Description = $"删除列 {column.Name} 从表 {tableChange.Entity.GetFullName()}"
                    });
                }
            }

            // 处理删除表
            foreach (var table in differences.TablesToDelete)
            {
                scripts.Add(new MigrationScript
                {
                    TableName = table.Name,
                    Operation = "DropTable",
                    Sql = GenerateDropTableScript(table),
                    Description = $"删除表 {table.GetFullName()}"
                });
            }

            return scripts;
        }

        /// <summary>
        /// 执行迁移脚本
        /// </summary>
        public virtual async Task ExecuteMigrationScriptsAsync(List<MigrationScript> scripts)
        {
            if (_options.ScriptOnly)
            {
                // 仅输出脚本，不执行
                foreach (var script in scripts)
                {
                    Console.WriteLine($"-- {script.Description}");
                    Console.WriteLine(script.Sql);
                    Console.WriteLine();
                }
                return;
            }

            // 开启事务，保证迁移的原子性
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var script in scripts)
                    {
                        Console.WriteLine($"执行: {script.Description}");
                        await _context.Database.ExecuteSqlRawAsync(script.Sql);
                    }

                    // 提交事务
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    await transaction.RollbackAsync();
                    throw new Exception($"执行迁移脚本失败: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 记录迁移历史
        /// </summary>
        public virtual async Task RecordMigrationHistoryAsync(List<MigrationScript> scripts)
        {
            foreach (var script in scripts)
            {
                string sql = $@"
                    INSERT INTO {EscapeIdentifier(_options.HistoryTableName)} 
                    (TableName, Operation, SqlScript, AppliedAt, Description)
                    VALUES 
                    ({CreateParameterName("tableName")}, {CreateParameterName("operation")}, {CreateParameterName("sql")}, {GetCurrentDateTimeFunction()}, {CreateParameterName("description")})
                ";

                var parameters = new object[]
                {
                    CreateParameter("tableName", script.TableName),
                    CreateParameter("operation", script.Operation),
                    CreateParameter("sql", script.Sql),
                    CreateParameter("description", script.Description)
                };

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
            }
        }

        /// <summary>
        /// 生成创建表的SQL脚本
        /// </summary>
        protected abstract string GenerateCreateTableScript(EntityDefinition entity);

        /// <summary>
        /// 生成添加列的SQL脚本
        /// </summary>
        protected abstract string GenerateAddColumnScript(EntityDefinition entity, PropertyDefinition property);

        /// <summary>
        /// 生成修改列的SQL脚本
        /// </summary>
        protected abstract string GenerateAlterColumnScript(EntityDefinition entity, ColumnPropertyPair columnPair);

        /// <summary>
        /// 生成删除列的SQL脚本
        /// </summary>
        protected abstract string GenerateDropColumnScript(EntityDefinition entity, ColumnDefinition column);

        /// <summary>
        /// 生成删除表的SQL脚本
        /// </summary>
        protected abstract string GenerateDropTableScript(TableDefinition table);

        /// <summary>
        /// 转义标识符
        /// </summary>
        protected abstract string EscapeIdentifier(string identifier);

        /// <summary>
        /// 创建参数名
        /// </summary>
        protected abstract string CreateParameterName(string name);

        /// <summary>
        /// 创建参数
        /// </summary>
        protected abstract object CreateParameter(string name, object value);

        /// <summary>
        /// 获取当前日期时间函数
        /// </summary>
        protected abstract string GetCurrentDateTimeFunction();
    }
}