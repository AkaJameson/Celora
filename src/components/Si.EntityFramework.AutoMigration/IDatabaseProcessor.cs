using System.Collections.Generic;
using System.Threading.Tasks;
using Si.EntityFramework.AutoMigration.Models;

namespace Si.EntityFramework.AutoMigration
{
    /// <summary>
    /// 数据库处理器接口
    /// </summary>
    public interface IDatabaseProcessor
    {
        /// <summary>
        /// 确保迁移历史表存在
        /// </summary>
        Task EnsureMigrationHistoryTableExistsAsync();
        
        /// <summary>
        /// 获取模型定义
        /// </summary>
        List<EntityDefinition> GetModelDefinitions();
        
        /// <summary>
        /// 获取数据库定义
        /// </summary>
        Task<List<TableDefinition>> GetDatabaseDefinitionsAsync();
        
        /// <summary>
        /// 比较差异
        /// </summary>
        SchemaDifference CompareDifferences(List<EntityDefinition> modelDefinitions, List<TableDefinition> databaseDefinitions);
        
        /// <summary>
        /// 生成迁移脚本
        /// </summary>
        List<MigrationScript> GenerateMigrationScripts(SchemaDifference differences);
        
        /// <summary>
        /// 执行迁移脚本
        /// </summary>
        Task ExecuteMigrationScriptsAsync(List<MigrationScript> scripts);
        
        /// <summary>
        /// 记录迁移历史
        /// </summary>
        Task RecordMigrationHistoryAsync(List<MigrationScript> scripts);
    }
} 