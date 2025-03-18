namespace Si.EntityFramework.IdentityServer.Models
{
    public class Session
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int userId { get; set; } = 0;
        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 账户
        /// </summary>
        public string Account { get; set; } = string.Empty;
        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        /// <summary>
        /// 用户角色
        /// </summary>
        public IEnumerable<string> Roles { get; set; }
    }
}
