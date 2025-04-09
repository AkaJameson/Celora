using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Si.EntityFramework.AutoMigration.Core
{
    public class DesignTimeService
    {
        private DbContext DbContext { get; set; }
        private IServiceProvider DesignService { get; set; }
        public DesignTimeService(DbContext dbContext)
        {
            DbContext = dbContext;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFrameworkDesignTimeServices();
            serviceCollection.AddDbContextDesignTimeServices(DbContext);
            DesignService = serviceCollection.BuildServiceProvider();
        }

        public IMigrationsScaffolder MigrationsScaffolder
        {
            get
            {
                return DesignService.GetRequiredService<IMigrationsScaffolder>();
            }
        }

        public IDatabaseModelFactory DatabaseModelFactory
        {
            get
            {
                return DesignService.GetRequiredService<IDatabaseModelFactory>();
            }
        }
        public IScaffoldingModelFactory ScaffoldingModelFactory
        {
            get
            {
                return DesignService.GetRequiredService<IScaffoldingModelFactory>();
            }
        }
        public IMigrationsAssembly MigrationsAssembly
        {
            get
            {
                return DbContext.GetInfrastructure().GetRequiredService<IMigrationsAssembly>();
            }
        }
        public IDesignTimeModel DesignTimeModel
        {
            get
            {
                return DbContext.GetInfrastructure().GetRequiredService<IDesignTimeModel>();
            }
        }
        public IModel Model
        {
            get
            {
                return DbContext.GetInfrastructure().GetRequiredService<IModel>();
            }
        }
        public IMigrationsModelDiffer ModelDiffer
        {
            get
            {
                return DbContext.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
            }
        }
        public IMigrationsSqlGenerator MigrationsSqlGenerator
        {
            get
            {
                return DbContext.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
            }
        }
    }
}
