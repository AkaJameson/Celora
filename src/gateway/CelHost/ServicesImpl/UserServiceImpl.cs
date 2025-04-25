using CelHost.Data;
using CelHost.Database;
using CelHost.Models.UserInfoModels;
using CelHost.Services;
using CelHost.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Si.Utilites.OperateResult;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CelHost.ServicesImpl
{
    /// <summary>
    /// 用户服务实现类
    /// </summary>
    public class UserServiceImpl : IUserService
    {
        private readonly HostContext _dbContext;
        private IConfiguration _configuration;
        private IHttpContextAccessor _httpContextAccessor;
        public UserServiceImpl(HostContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContext)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContext;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> Login(LoginModel loginModel)
        {
            loginModel.Account = SHA256Helper.Encrypt(loginModel.Account);
            loginModel.Password = SHA256Helper.Encrypt(loginModel.Password);
            var user = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.Account == loginModel.Account && u.Password == loginModel.Password);
            if (user == null)
            {
                return OperateResult.Failed("用户名或密码错误");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),

            };
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                Expires = DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"])),
                SigningCredentials = credentials,
                Claims = claims.ToDictionary(c => c.Type, c => (object)c.Value)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            _httpContextAccessor.HttpContext.Response.Cookies.Append("Access-Token", tokenHandler.WriteToken(token), new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"])),
                SameSite = SameSiteMode.Lax
            });
            return OperateResult.Successed();
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="resetPasswordModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> ResetPassword(ResetPsdModel resetPasswordModel)
        {
            resetPasswordModel.Account = SHA256Helper.Encrypt(resetPasswordModel.Account);
            resetPasswordModel.OldPassword = SHA256Helper.Encrypt(resetPasswordModel.Password);
            var user = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.Account == resetPasswordModel.Account && u.Password == resetPasswordModel.OldPassword);
            if (user == null)
            {
                return OperateResult.Failed("用户名或密码错误");
            }
            if (resetPasswordModel.Password != resetPasswordModel.ConfirmPassword)
            {
                return OperateResult.Failed("两次输入的密码不一致");
            }
            user.Password = SHA256Helper.Encrypt(resetPasswordModel.Password);
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            return OperateResult.Successed();
        }
        /// <summary>
        /// 初始化User
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<OperateResult> InitUser(IFormFile file)
        {
            if (Path.GetExtension(file.FileName).ToLower() != ".license")
                return OperateResult.Failed("文件格式不正确，仅支持 .license 文件");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            string content;
            using (var reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync();
            }

            UserLicenseModel license;
            try
            {
                license = JsonSerializer.Deserialize<UserLicenseModel>(content);
            }
            catch (Exception ex)
            {
                return OperateResult.Failed("授权文件解析失败：" + ex.Message);
            }

            // 检查必要字段
            if (string.IsNullOrEmpty(license.UserName) ||
                string.IsNullOrEmpty(license.Password) ||
                license.ExpireAt == default ||
                string.IsNullOrEmpty(license.Signature))
            {
                return OperateResult.Failed("授权文件缺少必要字段");
            }

            // 校验签名
            var rawData = $"{license.Account}|{license.UserName}|{license.Password}|{license.ExpireAt:O}";
            //期待签名
            var expectedSignature = StableAesCrypto.ComputeHMAC(rawData, "your-secret-key");

            if (license.Signature != expectedSignature)
            {
                return OperateResult.Failed("授权签名无效，文件可能被篡改");
            }

            if (license.ExpireAt < DateTime.UtcNow)
            {
                return OperateResult.Failed("授权已过期");
            }
            var result = await AddUser(license.Account, license.UserName, license.Password);
            if (!result.result)
            {
                return OperateResult.Failed(result.msg);
            }
            else
            {
                return OperateResult.Successed(result.msg);
            }
        }
        private async Task<(bool result, string msg)> AddUser(string account, string userName, string password)
        {
            var result = StableAesCrypto.GenerateKeyAndIV();
            var user = new User()
            {
                Account = StableAesCrypto.Encrypt(account, result.Key, result.IV),
                UserName = StableAesCrypto.Encrypt(userName, result.Key, result.IV),
                Password = StableAesCrypto.Encrypt(password, result.Key, result.IV),
                Key = result.Key,
                IV = result.IV
            };
            if (_dbContext.Set<User>().Any(p => p.UserName == userName))
            {
                return (false, "存在相同账户名称");
            }
            await _dbContext.Set<User>().AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return (true, "添加成功");

        }

    }
}
