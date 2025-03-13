using Si.EntityFramework.IdentityServer.Entitys;

namespace Si.EntityFramework.IdentityServer.Models
{
    /// <summary>
    /// 刷新令牌验证结果
    /// </summary>
    public class RefreshTokenValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 是否存在安全风险
        /// </summary>
        public bool SecurityRisk { get; set; }

        /// <summary>
        /// 刷新令牌对象
        /// </summary>
        public UserRefreshTokens RefreshToken { get; set; }
    }

}
