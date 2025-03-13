using Microsoft.IdentityModel.Tokens;
using Si.EntityFrame.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Si.EntityFrame.IdentityServer.Tools
{
    /// <summary>
    /// JWT令牌管理器
    /// </summary>
    public class JwtManager
    {
        private readonly JwtSettings _settings;

        public JwtManager(JwtSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            // 验证必要的设置
            if (string.IsNullOrEmpty(_settings.SecretKey))
                throw new ArgumentException("JWT密钥不能为空");

            if (string.IsNullOrEmpty(_settings.Issuer))
                throw new ArgumentException("JWT发行者不能为空");

            if (string.IsNullOrEmpty(_settings.Audience))
                throw new ArgumentException("JWT接收者不能为空");

            if (_settings.ExpirationMinutes <= 0)
                throw new ArgumentException("JWT过期时间必须大于0");
        }
        public JwtManager(string SecretKey, string Issuer, string Audience, int ExpirationMinutes)
        {
            // 验证必要的设置
            if (string.IsNullOrEmpty(SecretKey))
                throw new ArgumentException("JWT密钥不能为空");

            if (string.IsNullOrEmpty(Issuer))
                throw new ArgumentException("JWT发行者不能为空");

            if (string.IsNullOrEmpty(Audience))
                throw new ArgumentException("JWT接收者不能为空");

            if (ExpirationMinutes <= 0)
                throw new ArgumentException("JWT过期时间必须大于0");
            _settings = new JwtSettings()
            {
                SecretKey = SecretKey,
                Issuer = Issuer,
                Audience = Audience,
                ExpirationMinutes = ExpirationMinutes
            };
        }

        /// <summary>
        /// 为用户生成JWT令牌
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="roles">角色列表</param>
        /// <param name="permissions">权限列表</param>
        /// <returns>JWT令牌</returns>
        public string GenerateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim("name", user.Account),
                new Claim("security_stamp", user.SecurityStamp ?? Guid.NewGuid().ToString())
            };

            // 添加角色声明
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // 添加权限声明
            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }
            }

            // 创建安全密钥
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 创建JWT令牌
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 验证JWT令牌
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>声明主体，验证失败则返回null</returns>
        public ClaimsPrincipal ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

            try
            {
                // 验证参数
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // 不允许任何时钟偏差
                };

                // 验证令牌
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 验证JWT令牌(非日期)
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>声明主体，验证失败则返回null</returns>
        public ClaimsPrincipal ValidateTokenWithoutExpiretion(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                // 关键点：关闭生命周期验证，允许过期的Token通过验证
                ValidateLifetime = false
            };
            try
            {
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 刷新JWT令牌
        /// </summary>
        /// <param name="token">原JWT令牌</param>
        /// <returns>新的JWT令牌，原令牌失效则返回null</returns>
        public string RefreshToken(string token)
        {
            // 验证原令牌
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            // 获取原令牌的声明
            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var username = principal.FindFirst("name")?.Value;
            var securityStamp = principal.FindFirst("security_stamp")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                return null;

            // 创建用户对象
            var user = new User
            {
                Id = int.Parse(userId),
                Account = username,
                SecurityStamp = securityStamp
            };

            // 获取角色和权限
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = principal.FindAll("permission").Select(c => c.Value);

            // 生成新令牌
            return GenerateToken(user, roles, permissions);
        }

        /// <summary>
        /// 生成刷新令牌
        /// </summary>
        /// <returns>刷新令牌</returns>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}