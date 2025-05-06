using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Si.AccessControl.Entitys;

namespace Si.AccessControl.EntityConfigures
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(r => r.Name).HasMaxLength(256).IsRequired();
            builder.HasMany(x => x.Permissions).WithMany(x => x.Roles).UsingEntity(x => x.ToTable("RolePermissions"));
            builder.HasMany(x=>x.Users).WithMany(x=>x.Roles).UsingEntity(x=>x.ToTable("UserRoles"));
        }
    }
}
