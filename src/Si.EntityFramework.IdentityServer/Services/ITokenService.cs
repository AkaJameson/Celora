using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.IdentityServer.Entitys;

namespace Si.EntityFramework.IdentityServer.Services
{
    public interface ITokenService<T> where T : DbContext, new()
    {
        string GenerateRefreshToken(User user, TimeSpan VaildTime);
        string GenerateToken(User user);
        Task<(bool isVaild, string msg, string token, string refreshToken)> RefreshToken(string refreshTokenStr, TimeSpan VaildTime);
    }
}