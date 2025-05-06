using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Si.AccessControl.Entitys;

namespace Si.AccessControl.EntityConfigures
{
    public class PersonnelInfoConfiguration : IEntityTypeConfiguration<PersonnelInfo>
    {
        public void Configure(EntityTypeBuilder<PersonnelInfo> builder)
        {
            builder.HasOne(x => x.User).WithOne(x => x.PersonnelInfo).HasForeignKey<PersonnelInfo>(x => x.userId);
        }
    }
}
