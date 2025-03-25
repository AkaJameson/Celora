using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Si.EntityFramework.IdentityServer.Entitys;

namespace Si.EntityFramework.IdentityServer.EntitysConfiguration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(r => r.Name).HasMaxLength(256).IsRequired();
            builder.HasMany(x => x.Permissions).WithMany(x => x.Roles).UsingEntity(x => x.ToTable("RolePermissions"));
            builder.HasMany(x=>x.Users).WithMany(x=>x.Roles).UsingEntity(x=>x.ToTable("UserRoles"));
        }
    }
}
