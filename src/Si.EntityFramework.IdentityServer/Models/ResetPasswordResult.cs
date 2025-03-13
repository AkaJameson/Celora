namespace Si.EntityFramework.IdentityServer.Models
{
    /// <summary>
    /// 重置密码结果
    /// </summary>
    public class ResetPasswordResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }

}
