using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.Extensions.Logging;
using Si.EntityFramework.AutoMigration.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Si.EntityFramework.AutoMigration.Core
{
    public class MigrationExecuter
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
        private readonly ILogger<MigrationExecuter> _logger;
        public MigrationExecuter(ILogger<MigrationExecuter> _logger)
        {
            this._logger = _logger;
        }

        public async Task Migrate(DbContext context, AutoMigrationOptions options)
        {
            await semaphore.WaitAsync();
            try
            {
                var database = context.Database;
                var services = context.GetInfrastructure();

                var differ = services.GetRequiredService<IMigrationsModelDiffer>();
                var sqlGenerator = services.GetRequiredService<IMigrationsSqlGenerator>();
                var dbModelFactory = services.GetRequiredService<IDatabaseModelFactory>(); // 关键服务 ✅

                var dbConnection = context.Database.GetDbConnection();

                // 1️⃣ 获取数据库模型（旧结构）
                var dbModel = dbModelFactory.Create(dbConnection, new DatabaseModelFactoryOptions());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                semaphore.Release();
            }
        }
        public static void Migrate(DbContext context)
        {
            

            // 2️⃣ 构建 IModel 旧模型（模拟 snapshot），用 Scaffolding 工具
            var scaffolder = services.GetRequiredService<IReverseEngineerScaffolder>();
            var designTimeServices = new DesignTimeServicesBuilder(services, context.GetType(), context.Database.ProviderName, null)
                                        .Build(context.Database.ProviderName);

            var scaffoldingFactory = designTimeServices.GetRequiredService<IModelCodeGenerator>();

            var modelFactory = designTimeServices.GetRequiredService<IScaffoldingModelFactory>();
            var oldModel = modelFactory.Create(dbModel, false).Model;

            // 3️⃣ 当前模型
            var newModel = context.Model;

            // 4️⃣ 差异比对
            var operations = differ.GetDifferences(oldModel.GetRelationalModel(), newModel.GetRelationalModel());

            if (!operations.Any())
            {
                Console.WriteLine("[AutoMigration] 无变更。");
                return;
            }

            // 5️⃣ 生成 SQL 并执行
            var commands = sqlGenerator.Generate(operations);
            var sql = string.Join(";\n", commands.Select(c => c.CommandText)) + ";";

            Console.WriteLine("[AutoMigration] 执行迁移 SQL...");
            context.Database.ExecuteSqlRaw(sql);
            Console.WriteLine("[AutoMigration] 完成 ✅");
        }


    }
}
