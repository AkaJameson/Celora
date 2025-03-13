namespace Si.EntityFramework.IdentityServer.Models
{
    /// <summary>
    /// 注册结果
    /// </summary>
    public class RegisterResult
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
    }
}
