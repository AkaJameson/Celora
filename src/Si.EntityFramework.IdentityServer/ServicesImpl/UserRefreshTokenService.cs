using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Models;
using Si.EntityFramework.IdentityServer.Services;
using System.IO.Ports;

namespace Si.EntityFramework.IdentityServer.ServicesImpl
{
    public class UserRefreshTokenService : IUserRefreshTokenService
    {
        private readonly DbContext _dbContext;
        private readonly JwtManager jwtManager;
        private readonly int _refreshTokenLifetimeDays;
        private readonly IHttpContextAccessor _contextAccessor;
        /// <summary>
        /// 构造函数
        /// </summary>
        public UserRefreshTokenService(DbContext dbContext,
               JwtManager jwtManager, IHttpContextAccessor contextAccessor, int refreshTokenLifetimeDays = 30)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _refreshTokenLifetimeDays = refreshTokenLifetimeDays;
            this.jwtManager = jwtManager;
            _contextAccessor = contextAccessor;
        }
        /// <summary>
        /// 清除过期Token
        /// </summary>
        /// <returns></returns>
        public async Task<int> CleanupExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            var expiredTokens = await _dbContext.Set<UserRefreshTokens>()
                .Where(r => r.ExpiryTime < now)
                .ToListAsync();

            _dbContext.Set<UserRefreshTokens>().RemoveRange(expiredTokens);
            await _dbContext.SaveChangesAsync();

            
            return expiredTokens.Count;
        }

        public Task<UserRefreshTokens> CreateTokenAsync(int userId, string accessToken, string refreshToken)
        {
            // 生成随机令牌
            string tokenValue = jwtManager.GenerateRefreshToken();

            var token = UserRefreshTokens
        }

        public Task<IEnumerable<UserRefreshTokens>> GetUserActiveTokensAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RevokeTokenAsync(string token, string reason = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> RevokeUserTokensAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserRefreshTokens> UseRefreshTokenAsync(string token, string newJwtId, string device = null, string ipAddress = null)
        {
            throw new NotImplementedException();
        }

        public Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string token, string device = null, string ipAddress = null)
        {
            throw new NotImplementedException();
        }
    }
}
