using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Si.AccessControl.Entitys
{
    public class Permission
    {
        /// <summary>
        /// 权限Id
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 权限名称
        /// </summary>
        [Column("permessionname")]
        public string PermessionName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [Column("description")]
        public string Description { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("createtime")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public virtual IList<Role> Roles { get; set; }
    }
   
}
