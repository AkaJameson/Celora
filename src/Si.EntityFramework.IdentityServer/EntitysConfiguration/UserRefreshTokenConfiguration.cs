using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Si.EntityFramework.IdentityServer.Entitys;

namespace Si.EntityFramework.IdentityServer.EntitysConfiguration
{
    public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshTokens>
    {
        public void Configure(EntityTypeBuilder<UserRefreshTokens> builder)
        {
            builder.ToTable("UserRefreshToken");
            builder.HasKey(t => t.Id);

            builder.HasOne(t => t.User)
                   .WithMany(u => u.UserRefreshTokens)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
