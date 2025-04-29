using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Si.EntityFramework.AutoMigration.Core
{
    public class MigrationStepProcessor
    {
        private readonly DbContext dbContext;
        public MigrationStepProcessor(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public string JoinCommands(List<MigrationCommand> operations)
        {
            var providerName = dbContext.Database.ProviderName;
            var sqls = operations.Select(o => o.CommandText);

            switch (providerName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    return string.Join("", sqls);
                default:
                    return string.Join(";", sqls);
            }
        }

        public List<MigrationOperation> FilterMigrationOperations(List<MigrationOperation> operations)
        {
            operations = (from x in operations
                          where !(x is AddCheckConstraintOperation)
                          where !(x is AddForeignKeyOperation)
                          where !(x is AddPrimaryKeyOperation)
                          where !(x is AddUniqueConstraintOperation)
                          where !(x is CreateIndexOperation)
                          where !(x is DatabaseOperation)
                          where !(x is DeleteDataOperation)
                          where !(x is DropCheckConstraintOperation)
                          where !(x is DropForeignKeyOperation)
                          where !(x is DropIndexOperation)
                          where !(x is DropPrimaryKeyOperation)
                          where !(x is DropSchemaOperation)
                          where !(x is DropSequenceOperation)
                          where !(x is DropUniqueConstraintOperation)
                          where !(x is EnsureSchemaOperation)
                          where !(x is InsertDataOperation)
                          where !(x is AlterTableOperation)
                          where !(x is RenameTableOperation)
                          where !(x is DropTableOperation)
                          where !(x is RenameIndexOperation)
                          where !(x is AlterSequenceOperation)
                          where !(x is RenameSequenceOperation)
                          where !(x is RestartSequenceOperation)
                          where !(x is SqlOperation)
                          where !(x is UpdateDataOperation)
                          where !(x is DropColumnOperation)
                          where !(x is AlterColumnOperation)
                          where !(x is RenameColumnOperation)
                          select x).ToList();
            return operations;
        }
    }
}
