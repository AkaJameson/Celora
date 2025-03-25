using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Si.EntityFramework.IdentityServer.Configuration;
using Si.EntityFramework.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Models;
using Si.EntityFramework.IdentityServer.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Si.EntityFramework.IdentityServer.ServicesImpl
{
    public class TokenService<T> : ITokenService<T> where T : DbContext, new()
    {
        private readonly JwtSettings _jwtSettings;

        private DbContext Context;
        private string Key { get; set; } = "QCaZslluwwKv28yXSqBApklhqzgnTG0Et41FZuM4sNE=";
        private string IV { get; set; } = "Aubw812rCAi/dIPNgw86mA==";
        public TokenService(JwtSettings jwtSettings, DbContext context)
        {
            _jwtSettings = jwtSettings;
            Context = context;
        }
        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("Id",user.Id.ToString()),
                new Claim("Name",user.PersonnelInfo.Name??string.Empty),
                new Claim("Account",user.Account),
                new Claim("Phone",user.PersonnelInfo.Phone??string.Empty)
            };
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }
            var claimsDictionary = new Dictionary<string, object>();
            foreach (var claim in claims)
            {
                claimsDictionary.Add(claim.Type, claim.Value);
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer, // 设置 Token 的颁发者
                Audience = _jwtSettings.Audience, // 设置 Token 的接收者
                Expires = DateTime.Now.AddMinutes(_jwtSettings.ExpirationMinutes), // 设置过期时间
                SigningCredentials = credentials, // 使用签名凭证来签署 Token
                Claims = claimsDictionary // 设置 Token 中的 claims
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor); // 生成 Token
            return tokenHandler.WriteToken(token);
        }
        /// <summary>
        /// 生成RefreshToken
        /// </summary>
        /// <param name="user"></param>
        /// <param name="VaildTime">有效时长</param>
        /// <returns></returns>
        public string GenerateRefreshToken(User user, TimeSpan VaildTime)
        {
            var refreshToken = new RefreshTokenModel() { Deadline = DateTime.Now + VaildTime, UserId = user.Id };
            var refreshTokenStr = AESCrypto.Encrypt(JsonConvert.SerializeObject(refreshToken), Key, IV);
            return refreshTokenStr;
        }
        public async Task<(bool isVaild, string msg, string token, string refreshToken)> RefreshToken(string refreshTokenStr, TimeSpan VaildTime)
        {
            var refreshToken = JsonConvert.DeserializeObject<RefreshTokenModel>(AESCrypto.Decrypt(refreshTokenStr, Key, IV));
            if (refreshToken.Deadline > DateTime.Now)
            {
                return (false, "RefreshToken is expired!", null, null);
            }
            var user = await Context.Set<User>().FirstOrDefaultAsync(p => p.Id == refreshToken.UserId);
            if (user == null)
            {
                return (false, "User is not exist!", null, null);
            }
            var token = GenerateToken(user);
            var refreshTokenNew = GenerateRefreshToken(user, VaildTime);
            return (true, "RefreshToken is vaild!", token, refreshTokenNew);
        }
    }
}
