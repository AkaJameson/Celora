using Microsoft.IdentityModel.Tokens;
using Si.EntityFrame.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Si.EntityFramework.IdentityServer.ServicesImpl
{
    public class TokenService
    {
        private readonly JwtSettings _jwtSettings;
        public TokenService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
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
        /// 生成 Refresh Token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GenerateRefreshToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("Id",user.Id.ToString()),
            };
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
                Expires = DateTime.Now.AddDays(7), // 设置过期时间
                SigningCredentials = credentials, // 使用签名凭证来签署 Token
                Claims = claimsDictionary // 设置 Token 中的 claims
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor); // 生成 Token
            return tokenHandler.WriteToken(token);
        }
        public string RefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero // 可以设置允许的时间误差
            };
            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var securityToken);
                if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                {
                    var userId = principal.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
    }
}
