namespace Si.EntityFramework.IdentityServer.Models
{
    /// <summary>
    /// 登录结果
    /// </summary>
    public class LoginResult
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
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// 角色列表
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// 权限列表
        /// </summary>
        public List<string> Permissions { get; set; }
    }
}
