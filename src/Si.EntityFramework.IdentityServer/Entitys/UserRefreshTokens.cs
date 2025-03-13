using Si.EntityFrame.IdentityServer.Entitys;

namespace Si.EntityFramework.IdentityServer.Entitys
{
    public class UserRefreshTokens
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        public string AssessToken { get; set; }
        /// <summary>
        /// 刷新Token
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// User
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiryTime { get; set; }
        /// <summary>
        /// ip地址
        /// </summary>
        public string IpAddress { get; set; }
    }
}
