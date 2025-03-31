using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.AutoMigration.Processors;

namespace Si.EntityFramework.AutoMigration
{
    /// <summary>
    /// 提供DbContext自动迁移功能的扩展方法
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// 同步检查并更新数据库表结构
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="options">迁移选项</param>
        public static void AutoMigration(this DbContext context, AutoMigrationOptions options = null)
        {
            AutoMigrationAsync(context, options).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// 异步检查并更新数据库表结构
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="options">迁移选项</param>
        public static async Task AutoMigrationAsync(this DbContext context, AutoMigrationOptions options = null)
        {
            options ??= new AutoMigrationOptions();
            
            try
            {
                var provider = DatabaseProviderDetector.DetectProvider(context);
                
                // 创建适合当前数据库的处理器
                var processor = CreateDatabaseProcessor(context, provider, options);
                
                // 确保自动迁移历史表存在
                if (options.TrackHistory)
                {
                    await processor.EnsureMigrationHistoryTableExistsAsync();
                }
                
                // 获取模型中的实体信息
                var modelDefinitions = processor.GetModelDefinitions();
                
                // 获取数据库中的表结构
                var databaseDefinitions = await processor.GetDatabaseDefinitionsAsync();
                
                // 对比差异
                var differences = processor.CompareDifferences(modelDefinitions, databaseDefinitions);
                
                // 生成迁移脚本
                var scripts = processor.GenerateMigrationScripts(differences);
                
                if (scripts.Count == 0)
                {
                    Console.WriteLine("无需迁移，数据库模式与应用程序模型一致。");
                    return;
                }
                
                Console.WriteLine($"发现 {scripts.Count} 个迁移脚本需要执行。");
                
                // 执行迁移脚本
                await processor.ExecuteMigrationScriptsAsync(scripts);
                
                // 记录迁移历史
                if (options.TrackHistory)
                {
                    await processor.RecordMigrationHistoryAsync(scripts);
                }
                
                Console.WriteLine("自动迁移完成！");
            }
            catch (Exception ex)
            {
                if (options.ThrowOnError)
                {
                    throw;
                }
                
                Console.WriteLine($"自动迁移失败: {ex.Message}");
                
                if (options.DetailedErrors)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
        
        public static async Task AutoMigrationAsync<T>(this IServiceProvider serviceProvider, AutoMigrationOptions options = null) where T :DbContext
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<T>();
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if(context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            await AutoMigrationAsync(context, options);
        }
        public static void AutoMigration<T>(this IServiceProvider serviceProvider, AutoMigrationOptions options = null) where T : DbContext
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<T>();
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            AutoMigration(context, options);
        }
        private static IDatabaseProcessor CreateDatabaseProcessor(
            DbContext context, 
            DatabaseProviderType provider,
            AutoMigrationOptions options)
        {
            switch (provider)
            {
                case DatabaseProviderType.SqlServer:
                    return new SqlServerProcessor(context, options);
                case DatabaseProviderType.Sqlite:
                    return new SqliteProcessor(context, options);
                case DatabaseProviderType.MySql:
                    return new MySqlProcessor(context, options);
                case DatabaseProviderType.PostgreSql:
                    return new PostgreSqlProcessor(context, options);
                default:
                    throw new NotSupportedException($"不支持的数据库类型: {provider}");
            }
        }
    }
} 