using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Si.EntityFramework.IdentityServer.Entitys;

namespace Si.EntityFramework.IdentityServer.EntitysConfiguration
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        void IEntityTypeConfiguration<Permission>.Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PermessionName).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(100);
        }
    }
}
