using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using Pomelo.EntityFrameworkCore.MySql.Design.Internal;

namespace Si.EntityFramework.AutoMigration.Core
{
    public class DesignTimeService
    {
        private DbContext _dbContext { get; set; }
        private static IServiceProvider _serviceProvider { get; set; }
        private IDesignTimeModel _designTimeModel { get; set; }
        public DesignTimeService(DbContext dbContext)
        {
            _dbContext = dbContext;
            var originalServices = ((IInfrastructure<IServiceProvider>)dbContext).Instance;
            _designTimeModel = originalServices.GetRequiredService<IDesignTimeModel>();
            // 获取数据库提供者名称
            var providerName = _dbContext.Database.ProviderName;

            // 创建服务集合
            var services = new ServiceCollection();

            // 注册基础服务
            services.AddEntityFrameworkDesignTimeServices();

            // 添加数据库特定服务
            AddDatabaseSpecificServices(services, providerName);

            // 合并原始上下文服务
            services.AddSingleton(_ => _dbContext.GetService<IDesignTimeModel>());
            services.AddSingleton(_ => _dbContext.Model);

            _serviceProvider = services.BuildServiceProvider();
        }
        private void AddDatabaseSpecificServices(IServiceCollection services, string providerName)
        {
            switch (providerName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);
                    break;

                case "Pomelo.EntityFrameworkCore.MySql":
                    new MySqlDesignTimeServices().ConfigureDesignTimeServices(services);
                    break;

                case "Microsoft.EntityFrameworkCore.Sqlite":
                    new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
                    break;

                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    new NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported database provider: {providerName}");
            }
        }

        public IMigrationsScaffolder MigrationsScaffolder
        {
            get
            {
                return _serviceProvider.GetRequiredService<IMigrationsScaffolder>();
            }
        }

        public IDatabaseModelFactory DatabaseModelFactory
        {
            get
            {
                return _serviceProvider.GetRequiredService<IDatabaseModelFactory>();
            }
        }
        public IScaffoldingModelFactory ScaffoldingModelFactory
        {
            get
            {
                return _serviceProvider.GetRequiredService<IScaffoldingModelFactory>();
            }
        }
        public IMigrationsModelDiffer ModelDiffer
        {
            get
            {
                return _dbContext.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
            }
        }
        public IMigrationsSqlGenerator MigrationsSqlGenerator
        {
            get
            {
                return _dbContext.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
            }
        }
        public IModel Model => _designTimeModel.Model;
        public IRelationalModel CodeModel
           => _designTimeModel.Model.GetRelationalModel();
    }
}
