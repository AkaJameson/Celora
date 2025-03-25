using System.ComponentModel.DataAnnotations.Schema;

namespace Si.EntityFramework.IdentityServer.Entitys
{
    public class Role
    {
        public int Id { get; set; }
        [Column("RoleName")]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreateTime { get; set; }
        public virtual IList<Permission> Permissions { get; set; }
        public virtual IList<User> Users { get; set; }
    }
}
