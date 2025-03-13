using Si.EntityFramework.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Models;

namespace Si.EntityFramework.IdentityServer.Services
{
    public interface IUserRefreshTokenService
    {
        /// <summary>
        /// 创建令牌
        /// </summary>
        Task<UserRefreshTokens> CreateTokenAsync(int userId,string accessToken,string refreshToken);

        /// <summary>
        /// 验证刷新令牌
        /// </summary>
        Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string token, string device = null, string ipAddress = null);

        /// <summary>
        /// 使用刷新令牌并生成新的刷新令牌
        /// </summary>
        Task<UserRefreshTokens> UseRefreshTokenAsync(string token, string newJwtId, string device = null, string ipAddress = null);

        /// <summary>
        /// 撤销令牌
        /// </summary>
        Task<bool> RevokeTokenAsync(string token, string reason = null);

        /// <summary>
        /// 撤销用户的所有令牌
        /// </summary>
        Task<int> RevokeUserTokensAsync(int userId);

        /// <summary>
        /// 清理过期的令牌
        /// </summary>
        Task<int> CleanupExpiredTokensAsync();

        /// <summary>
        /// 获取用户的所有有效令牌
        /// </summary>
        Task<IEnumerable<UserRefreshTokens>> GetUserActiveTokensAsync(Guid userId);
    }
}
