namespace Si.EntityFramework.IdentityServer.Models
{
    public class TokenInfo
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public long ExpireTime { get; set; }
    }
}
