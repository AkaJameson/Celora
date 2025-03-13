namespace Si.EntityFramework.IdentityServer.Models
{

    /// <summary>
    /// 验证结果
    /// </summary>
    internal class ValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}
