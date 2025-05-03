using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Server.Data
{
    /// <summary>
    /// 管理页面用户信息表
    /// </summary>
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Account { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string? Description { get; set; }
        /// <summary>
        /// 登录锁定
        /// </summary>
        public bool LockEnable { get; set; } = false;
        public DateTime? LockTime { get; set; }
        public bool IsLock { get; set; } = false;
        [Required]
        public string Key { get; set; }
        [Required]
        public string IV { get; set; }
        [Required]
        public string Hash { get; set; }
    }
}
