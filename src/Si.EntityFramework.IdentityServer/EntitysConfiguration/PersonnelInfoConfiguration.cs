using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Si.EntityFramework.IdentityServer.Entitys;

namespace Si.EntityFramework.IdentityServer.EntitysConfiguration
{
    public class PersonnelInfoConfiguration : IEntityTypeConfiguration<PersonnelInfo>
    {
        public void Configure(EntityTypeBuilder<PersonnelInfo> builder)
        {
            builder.ToTable("BasePersonnelInfo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasOne(x => x.User).WithOne(x => x.PersonnelInfo).HasForeignKey<PersonnelInfo>(x => x.userId);
        }
    }
}
