using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.Logging;
using Si.EntityFramework.AutoMigration.Configuration;

namespace Si.EntityFramework.AutoMigration.Core
{
    public class MigrationExecuter
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private DesignTimeService designTimeService;
        private readonly ILogger<MigrationExecuter> _logger;
        private readonly DbContext _context;
        public MigrationExecuter(ILogger<MigrationExecuter> logger, DbContext context)
        {
            _logger = logger;
            _context = context;
            designTimeService = new DesignTimeService(context);
        }
        public async Task Migrate(DbContext context, AutoMigrationOptions options)
        {
            await semaphore.WaitAsync();
            try
            {
                var database = context.Database;
                var connection = database.GetDbConnection();
                IRelationalModel databaseModel = null;
                try
                {
                    var databaseModelFactory = designTimeService.DatabaseModelFactory;
                    var databaseModelOption = new DatabaseModelFactoryOptions();
                    var dbModel = databaseModelFactory.Create(connection, databaseModelOption);
                    var scaffoldingModelFactory = designTimeService.ScaffoldingModelFactory;
                    databaseModel = scaffoldingModelFactory.Create(dbModel, true).GetRelationalModel();

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read database model, assuming empty database");
                }
                var codeModel = designTimeService.Model.GetRelationalModel();
                var modelDiffer = designTimeService.ModelDiffer;
                var operations = modelDiffer.GetDifferences(databaseModel, codeModel);
                if (!operations.Any())
                {
                    _logger.LogInformation("No pending model changes detected");
                    return;
                }
                var commands = designTimeService.MigrationsSqlGenerator.Generate(operations, designTimeService.Model);

                using var transaction = await database.BeginTransactionAsync();
                try
                {
                    foreach (var command in commands)
                    {
                        await database.ExecuteSqlRawAsync(
                            command.CommandText);
                    }
                    await transaction.CommitAsync();
                    _logger.LogInformation("Database migration completed successfully. Applied {Count} operations", operations.Count);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Migration transaction failed");
                }
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
    }
}
