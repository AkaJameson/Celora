using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Core.Abstractions;
using Si.EntityFramework.Extension.Data.Configurations;
using System.Linq.Expressions;

namespace Si.EntityFramework.Extension.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        private DbOptions options;
        protected ApplicationDbContext(IServiceProvider serviceProvider, DbContextOptions options): base(options)
        {
            var dbOptionProvider = serviceProvider.GetRequiredService<DbOptionsProvider>();
            if (options == null)
            {
                throw new Exception("DbContextOptions is null");
            }
            this.options = dbOptionProvider[GetType().Name];
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (options.EnableGlobalFilters && options.EnableMultiTenant)
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    if (typeof(ITenant).IsAssignableFrom(entityType.ClrType) &&
                        !options.IgnoredMultiTenantTypes.Contains(entityType.ClrType))
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var tenantProperty = Expression.Property(parameter, nameof(ITenant.TenantId));
                        var tenantValue = Expression.Constant(options.CurrentTenantIdProvider?.Invoke()?.TenantId ?? string.Empty);
                        var comparison = Expression.Equal(tenantProperty, tenantValue);
                        var lambda = Expression.Lambda(comparison, parameter);
                        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                    }
                }
            }
            base.OnModelCreating(modelBuilder);
        }
        public override int SaveChanges()
        {
            ApplyFeatures();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyFeatures();
            return base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyFeatures();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        private void ApplyFeatures()
        {
            if (options.EnableSnowflakeId)
            {
                ApplySnowflakeId();
            }

            if (options.EnableSoftDelete)
            {
                UpdateSoftDeleteState();
            }

            if (options.EnableSoftDelete)
            {
                ApplyAuditInfo();
            }
        }

        private void ApplySnowflakeId()
        {

            var shortEntrys = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is IShortId);
            if (shortEntrys.Count() != 0 && options.ShortIdGenerator == null)
            {
                throw new Exception("ShortIdGenerator is null");
            }
            foreach (var entry in shortEntrys)
            {
                if (entry.Entity is IShortId entity)
                {
                    entity.Id = options.ShortIdGenerator.NextId();
                }
            }
            var longEntrys = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is ILongId);
            if (longEntrys.Count() != 0 && options.LongIdGenerator == null)
            {
                throw new Exception("LongIdGenerator is Null");
            }
            foreach (var entry in longEntrys)
            {
                if (entry.Entity is ILongId entity)
                {
                    entity.Id = options.LongIdGenerator.NextId();
                }
            }
        }

        private void UpdateSoftDeleteState()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is ISoftDelete softDelete)
                {
                    switch (entry.State)
                    {
                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            softDelete.IsDeleted = true;
                            softDelete.DeletedTime = DateTime.Now;
                            break;
                    }
                }
            }
        }
        private void ApplyAuditInfo()
        {
            var userId = options.CurrentUserIdProvider?.Invoke()?.Id.ToString() ?? "System";
            var entries = ChangeTracker.Entries().ToList();

            foreach (var entry in entries)
            {
                if (entry.Entity is ICreationAudited creationAudited && entry.State == EntityState.Added)
                {
                    creationAudited.CreatedBy = userId;
                    creationAudited.CreatedTime = DateTime.Now;
                }

                if (entry.Entity is IModificationAudited modificationAudited && entry.State == EntityState.Modified)
                {
                    modificationAudited.LastModifiedBy = userId;
                    modificationAudited.LastModifiedTime = DateTime.Now;
                }

                if (entry.Entity is IFullAudited fullAudited && entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    fullAudited.IsDeleted = true;
                    fullAudited.DeletedTime = DateTime.Now;
                    fullAudited.DeletedBy = userId;
                }
            }
        }
    }
}
