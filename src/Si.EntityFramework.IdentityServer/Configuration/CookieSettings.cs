using Microsoft.AspNetCore.Http;

namespace Si.EntityFramework.IdentityServer.Configuration
{
    /// <summary>
    /// Cookie设置
    /// </summary>
    public class CookieSettings
    {
        /// <summary>
        /// Cookie名称
        /// </summary>
        public string CookieName { get; set; } = "Si.Identity.Cookie";

        /// <summary>
        /// 过期时间（分钟）
        /// </summary>
        public int ExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// 加密密钥
        /// </summary>
        public string EncryptionKey { get; set; } = "YourEncryptionKeyMustBeAtLeast32Chars!";

        /// <summary>
        /// 是否设置为HttpOnly
        /// </summary>
        public bool HttpOnly { get; set; } = true;

        /// <summary>
        /// 是否设置为Secure
        /// </summary>
        public bool Secure { get; set; } = true;

        /// <summary>
        /// Cookie的域
        /// </summary>
        public string Domain { get; set; } = null;

        /// <summary>
        /// Cookie的路径
        /// </summary>
        public string Path { get; set; } = "/";

        /// <summary>
        /// Cookie的SameSite属性
        /// </summary>
        public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
    }

}
