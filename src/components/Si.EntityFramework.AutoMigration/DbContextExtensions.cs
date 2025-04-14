using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Si.EntityFramework.AutoMigration.Configuration;
using Si.EntityFramework.AutoMigration.Core;

namespace Si.EntityFramework.AutoMigration
{
    /// <summary>
    /// 提供DbContext自动迁移功能的扩展方法
    /// </summary>
    public static class DbContextExtensions
    {
        public static void AddAutoMigrationProvider(this IServiceCollection services)
        {
            services.AddScoped<MigrationExecuter>();
            services.AddScoped<MigrationStepProcessor>();
        }
        /// <summary>
        /// 同步检查并更新数据库表结构
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="options">迁移选项</param>
        internal static void AutoMigration(this DbContext context, IServiceProvider sp, AutoMigrationOptions options = null)
        {
            AutoMigrationAsync(context, sp, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步检查并更新数据库表结构
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="options">迁移选项</param>
        internal static async Task AutoMigrationAsync(this DbContext context, IServiceProvider sp, AutoMigrationOptions options = null)
        {
            options = options ?? new AutoMigrationOptions();
            var executer = sp.GetRequiredService<MigrationExecuter>();
            await executer.Migrate(context, options);
        }

        public static async Task AutoMigrationAsync<T>(this WebApplication app, AutoMigrationOptions options = null) where T : DbContext
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<T>();
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            await AutoMigrationAsync(context, scope.ServiceProvider, options);
        }
        public static void AutoMigration<T>(this WebApplication app, AutoMigrationOptions options = null) where T : DbContext
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<T>();
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            AutoMigration(context, scope.ServiceProvider, options);
        }
    }
}