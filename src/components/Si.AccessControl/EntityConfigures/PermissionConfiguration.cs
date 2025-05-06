using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Si.AccessControl.Entitys;

namespace Si.AccessControl.EntityConfigures
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        void IEntityTypeConfiguration<Permission>.Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.Property(x => x.PermessionName).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(100);
        }
    }
}
