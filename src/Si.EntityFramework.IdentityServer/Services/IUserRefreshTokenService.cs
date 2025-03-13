using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Models;

namespace Si.EntityFramework.IdentityServer.ServicesImpl
{
    public interface IUserRefreshTokenService<T> where T :DbContext, new()
    {
        /// <summary>
        /// 清除所有过期Token
        /// </summary>
        /// <returns></returns>
        Task<int> CleanupExpiredTokensAsync();
        /// <summary>
        /// 创建令牌
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<TokenInfo> CreateTokenAsync(int userId);
        /// <summary>
        /// 获取用户的Token凭证信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserRefreshTokens> GetUserActiveTokensAsync(int userId);
        /// <summary>
        /// 撤销令牌
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> RevokeTokenAsync(int userId);
        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        Task<TokenInfo> UseRefreshTokenAsync(int userId, string refreshToken);
    }
}