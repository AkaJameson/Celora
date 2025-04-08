using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Si.EntityFramework.AutoMigration.Core
{
    public class MigrationOptions
    {
        private DbContext dbContext;
        public MigrationOptions(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        private IServiceProvider ServiceProvider
        {
            get
            {
                return dbContext.GetInfrastructure();
            }
        }
        /// <summary>
        /// 获取数据库模型工厂
        /// </summary>
        /// <returns></returns>
        public IDatabaseModelFactory DatabaseModelFactory
        {
            get
            {
                return ServiceProvider.GetRequiredService<IDatabaseModelFactory>();
            }
        }
        /// <summary>
        /// 获取迁移脚本
        /// </summary>
        public IMigrationsAssembly MigrationsAssembly
        {
            get
            {
                return ServiceProvider.GetRequiredService<IMigrationsAssembly>();
            }
        }
        public IDesignTimeModel DesignTimeModel
        {
            get
            {
                return ServiceProvider.GetRequiredService<IDesignTimeModel>();
            }
        }
        public IModel Model
        {
            get
            {
                return this.DesignTimeModel.Model;
            }
        }

        public IMigrationsModelDiffer ModelDiffer
        {
            get
            {
                return ServiceProvider.GetRequiredService<IMigrationsModelDiffer>();
            }
        }
        
        public
    }
}
