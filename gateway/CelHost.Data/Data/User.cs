using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CelHost.Data.Data
{
    /// <summary>
    /// 管理页面用户信息表
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// 登录锁定
        /// </summary>
        public bool LockEnable { get; set; } = false;
        public DateTime? LockTime { get; set; }
        public bool IsLock { get; set; } = false;
    }
}
