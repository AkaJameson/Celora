using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Si.EntityFrame.IdentityServer.Entitys;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Configuration;
using Si.EntityFramework.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Models;
using Si.EntityFramework.IdentityServer.Services;
using System.Data;
using System.IO.Ports;
using System.Security;

namespace Si.EntityFramework.IdentityServer.ServicesImpl
{
    public class UserRefreshTokenService<T> : IUserRefreshTokenService<T> where T : DbContext, new()
    {
        private readonly T _dbContext;
        private readonly JwtManager jwtManager;
        private readonly int _refreshTokenLifetimeDays;
        private readonly IRolePermissionService<T> _rolePermissionService;
        /// <summary>
        /// 构造函数
        /// </summary>
        public UserRefreshTokenService(T dbContext,
               JwtManager jwtManager,
               IRolePermissionService<T> rolePermissionService
            , int refreshTokenLifetimeDays = 30)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _refreshTokenLifetimeDays = refreshTokenLifetimeDays;
            this.jwtManager = jwtManager;
            _rolePermissionService = rolePermissionService;
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

        public async Task<TokenInfo> CreateTokenAsync(int userId)
        {
            var user = await _dbContext.Set<User>().Where(p => p.Id == userId).Include(p => p.Roles).ThenInclude(p => p.Permissions).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            var roles = await _rolePermissionService.GetRolesForUserAsync(userId);
            var permission = await _rolePermissionService.GetPermissionsForUserAsync(userId);

            var userRefreshToken = await _dbContext.Set<UserRefreshTokens>().FirstOrDefaultAsync(p => p.Id == userId);
            var accessToken = jwtManager.GenerateToken(user, roles.Select(p => p.Name), permission.Select(p => p.PermessionName));
            var refreshToken = jwtManager.GenerateRefreshToken();
            if (userRefreshToken == null)
            {
                var newuserRefreshToken = new UserRefreshTokens
                {
                    RefreshToken = refreshToken,
                    ExpiryTime = DateTime.Now.AddDays(_refreshTokenLifetimeDays),
                    UserId = user.Id,
                };
                await _dbContext.Set<UserRefreshTokens>().AddAsync(newuserRefreshToken);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                userRefreshToken.RefreshToken = refreshToken;
                await _dbContext.SaveChangesAsync();
            }
            return new TokenInfo
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpireTime = (DateTime.Now.Add(jwtManager.GetExpireTime()).Ticks)
            };
        }
        public async Task<TokenInfo> UseRefreshTokenAsync(int userId, string refreshToken)
        {
            var userRefreshToken = await _dbContext.Set<UserRefreshTokens>()
                .Where(p => p.Id == userId && p.RefreshToken == refreshToken)
                .Include(p => p.User)
                .FirstOrDefaultAsync();
            if (userRefreshToken == null)
            {
                return null;
            }
            else
            {
                var roles = await _rolePermissionService.GetRolesForUserAsync(userId);
                var permission = await _rolePermissionService.GetPermissionsForUserAsync(userId);
                var accessToken = jwtManager.GenerateToken(userRefreshToken.User, roles.Select(p => p.Name), permission.Select(p => p.PermessionName));
                return new TokenInfo
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpireTime = (DateTime.Now.Add(jwtManager.GetExpireTime()).Ticks)
                };
            }
        }
        public async Task<UserRefreshTokens> GetUserActiveTokensAsync(int userId)
        {
            return await _dbContext.Set<UserRefreshTokens>().FirstOrDefaultAsync(p => p.Id == userId);
        }

        public async Task<bool> RevokeTokenAsync(int userId)
        {
            var user = await _dbContext.Set<UserRefreshTokens>().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null) { return false; }
            _dbContext.Set<UserRefreshTokens>().Remove(user);
            return true;
        }

    }
}
