using Microsoft.AspNetCore.Http;
using Si.EntityFrame.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Si.EntityFrame.IdentityServer.Tools
{
    /// <summary>
    /// Cookie管理器
    /// </summary>
    public class CookieManager
    {
        private readonly CookieSettings _settings;

        public CookieManager(CookieSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            // 验证必要的设置
            if (string.IsNullOrEmpty(_settings.CookieName))
                throw new ArgumentException("Cookie名称不能为空");

            if (string.IsNullOrEmpty(_settings.EncryptionKey))
                throw new ArgumentException("Cookie加密密钥不能为空");
        }

        /// <summary>
        /// 设置认证Cookie
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <param name="user">用户</param>
        /// <param name="roles">角色列表</param>
        /// <param name="permissions">权限列表</param>
        public void SetAuthCookie(HttpContext context, User user, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // 创建认证数据
            var authData = new Dictionary<string, object>
            {
                { "sub", user.Id.ToString() },
                { "name", user.Account },
                { "security_stamp", user.SecurityStamp ?? Guid.NewGuid().ToString() },
                { "exp", DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(_settings.ExpirationMinutes)).ToUnixTimeSeconds() }
            };

            // 添加角色和权限
            if (roles != null && roles.Any())
            {
                authData["roles"] = roles.ToList();
            }

            if (permissions != null && permissions.Any())
            {
                authData["permissions"] = permissions.ToList();
            }

            // 序列化和加密
            var json = JsonSerializer.Serialize(authData);
            var encryptedData = EncryptData(json);

            // 设置Cookie
            var options = new CookieOptions
            {
                HttpOnly = _settings.HttpOnly,
                Secure = _settings.Secure,
                SameSite = _settings.SameSite,
                Expires = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(_settings.ExpirationMinutes))
            };

            if (!string.IsNullOrEmpty(_settings.Domain))
            {
                options.Domain = _settings.Domain;
            }

            context.Response.Cookies.Append(_settings.CookieName, encryptedData, options);
        }
        /// <summary>
        /// 移除认证Cookie
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        public void RemoveAuthCookie(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // 删除Cookie
            var options = new CookieOptions
            {
                HttpOnly = _settings.HttpOnly,
                Secure = _settings.Secure,
                SameSite = _settings.SameSite
            };

            if (!string.IsNullOrEmpty(_settings.Domain))
            {
                options.Domain = _settings.Domain;
            }

            context.Response.Cookies.Delete(_settings.CookieName, options);
        }

        /// <summary>
        /// 获取认证Cookie中的声明主体
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>声明主体，未认证则返回null</returns>
        public ClaimsPrincipal GetPrincipalFromCookie(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // 获取Cookie值
            if (!context.Request.Cookies.TryGetValue(_settings.CookieName, out var cookieValue) ||
                string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

            try
            {
                // 解密和反序列化
                var json = DecryptData(cookieValue);
                var authData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                // 检查是否过期
                if (authData.TryGetValue("exp", out var expElement) &&
                    expElement.ValueKind == JsonValueKind.Number)
                {
                    var expTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
                    if (expTime < DateTimeOffset.UtcNow)
                    {
                        // Cookie已过期，删除
                        RemoveAuthCookie(context);
                        return null;
                    }
                }
                else
                {
                    // 无过期时间，视为无效
                    return null;
                }

                // 创建声明
                var claims = new List<Claim>();

                // 添加基本声明
                if (authData.TryGetValue("sub", out var subElement) &&
                    subElement.ValueKind == JsonValueKind.String)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, subElement.GetString()));
                }

                if (authData.TryGetValue("name", out var nameElement) &&
                    nameElement.ValueKind == JsonValueKind.String)
                {
                    claims.Add(new Claim(ClaimTypes.Name, nameElement.GetString()));
                }

                if (authData.TryGetValue("security_stamp", out var stampElement) &&
                    stampElement.ValueKind == JsonValueKind.String)
                {
                    claims.Add(new Claim("security_stamp", stampElement.GetString()));
                }

                // 添加角色声明
                if (authData.TryGetValue("roles", out var rolesElement) &&
                    rolesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        if (role.ValueKind == JsonValueKind.String)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.GetString()));
                        }
                    }
                }

                // 添加权限声明
                if (authData.TryGetValue("permissions", out var permsElement) &&
                    permsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var perm in permsElement.EnumerateArray())
                    {
                        if (perm.ValueKind == JsonValueKind.String)
                        {
                            claims.Add(new Claim("permission", perm.GetString()));
                        }
                    }
                }

                // 创建身份
                var identity = new ClaimsIdentity(claims, "Cookie");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                // 处理错误，删除无效Cookie
                RemoveAuthCookie(context);
                return null;
            }
        }

        /// <summary>
        /// 检查用户是否已认证
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>是否已认证</returns>
        public bool IsAuthenticated(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // 获取声明主体
            var principal = GetPrincipalFromCookie(context);
            return principal?.Identity?.IsAuthenticated ?? false;
        }

        /// <summary>
        /// 检查用户是否具有指定角色
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <param name="role">角色名称</param>
        /// <returns>是否具有角色</returns>
        public bool IsInRole(HttpContext context, string role)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("角色名称不能为空", nameof(role));

            // 获取声明主体
            var principal = GetPrincipalFromCookie(context);
            return principal?.IsInRole(role) ?? false;
        }

        /// <summary>
        /// 检查用户是否具有指定权限
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <param name="permission">权限名称</param>
        /// <returns>是否具有权限</returns>
        public bool HasPermission(HttpContext context, string permission)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(permission))
                throw new ArgumentException("权限名称不能为空", nameof(permission));

            // 获取声明主体
            var principal = GetPrincipalFromCookie(context);
            return principal?.HasClaim("permission", permission) ?? false;
        }

        #region 私有方法

        /// <summary>
        /// 加密数据
        /// </summary>
        private string EncryptData(string data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKeyFromPassword(_settings.EncryptionKey);
                aes.IV = new byte[16]; // 使用全零IV，简化处理

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(data);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        private string DecryptData(string encryptedData)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKeyFromPassword(_settings.EncryptionKey);
                aes.IV = new byte[16]; // 使用全零IV，简化处理

                var bytes = Convert.FromBase64String(encryptedData);

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(bytes))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 从密码派生密钥
        /// </summary>
        private byte[] DeriveKeyFromPassword(string password)
        {
            // 使用固定盐值，简化处理
            byte[] salt = Encoding.UTF8.GetBytes("Si.Identity.Fixed.Salt");

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32); // 256位密钥
            }
        }

        #endregion
    }
}