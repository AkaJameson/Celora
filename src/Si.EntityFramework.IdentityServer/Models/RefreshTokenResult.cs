namespace Si.EntityFramework.IdentityServer.Models
{
    /// <summary>
    /// 刷新令牌结果
    /// </summary>
    public class RefreshTokenResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 访问令牌
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
