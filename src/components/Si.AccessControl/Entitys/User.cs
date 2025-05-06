using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Si.AccessControl.Entitys
{
    public class User
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 账户
        /// </summary>
        [Column("account")]
        public string Account { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Column("password")]
        public string Password { get; set; }
        /// <summary>
        /// (登录锁定)暂时锁定
        /// </summary>
        [Column("loginlock")]
        public bool LoginLock { get; set; }
        /// <summary>
        /// 锁定截止时间
        /// </summary>
        [Column("lockoutenddateutc")]
        public DateTime LockoutEndDateUtc { get; set; } = DateTime.MinValue;
        /// <summary>
        /// 密钥
        /// </summary>
        [Column("securitystamp")]
        public string? SecurityStamp { get; set; }
        /// <summary>
        /// 密码历史记录
        /// </summary>
        [Column("historypassword")]
        public string? HistoryPassword { get; set; }
        /// <summary>
        /// 是否锁定
        /// </summary>
        [Column("lock")]
        public bool Lock { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public virtual IList<Role> Roles { get; set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public virtual PersonnelInfo PersonnelInfo { get; set; }
        /// <summary>
        /// 扩展字段
        /// </summary>
        public string? Reserved1 { get; set; }
        public string? Reserved2 { get; set; }
        public string? Reserved3 { get; set; }
        public string? Reserved4 { get; set; }
        public string? Reserved5 { get; set; }
    }

}
