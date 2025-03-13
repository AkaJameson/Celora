namespace Si.EntityFramework.IdentityServer.Configuration
{
    /// <summary>
    /// Si身份认证选项
    /// </summary>
    public class IdentityOptions
    {
        public AuthorizationType AuthorizationType { get; set; }
        /// <summary>
        /// JWT设置
        /// </summary>
        public JwtSettings JwtSettings { get; set; } = new JwtSettings();

        /// <summary>
        /// Cookie设置
        /// </summary>
        public CookieSettings CookieSettings { get; set; } = new CookieSettings();

        /// <summary>
        /// 密码设置
        /// </summary>
        public PasswordSettings PasswordSettings { get; set; } = new PasswordSettings();
    }
    public enum AuthorizationType
    {
        /// <summary>
        /// Cookies
        /// </summary>
        Cookies,
        /// <summary>
        /// JWTToken
        /// </summary>
        Jwt
    }
}
