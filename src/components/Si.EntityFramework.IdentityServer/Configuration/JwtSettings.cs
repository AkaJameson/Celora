namespace Si.EntityFramework.IdentityServer.Configuration
{
    /// <summary>
    /// JWT设置
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; } = "YourSecretKeyHereMustBeAtLeast32Bytes!";

        /// <summary>
        /// 颁发者
        /// </summary>
        public string Issuer { get; set; } = "Si.Identity";

        /// <summary>
        /// 接收者
        /// </summary>
        public string Audience { get; set; } = "Si.Identity.Clients";

        /// <summary>
        /// 过期时间（分钟）
        /// </summary>
        public int ExpirationMinutes { get; set; } = 60;
    }

}
