using CelHost.Data;
using CelHost.Database;
using CelHost.Models;
using CelHost.Services;
using CelHost.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Si.Utilites.OperateResult;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            loginModel.Account = Md5Helper.GetMd5Hash(loginModel.Account);
            loginModel.Password = Md5Helper.GetMd5Hash(loginModel.Password);
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
        public async Task<OperateResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            resetPasswordModel.Account = Md5Helper.GetMd5Hash(resetPasswordModel.Account);
            resetPasswordModel.OldPassword = Md5Helper.GetMd5Hash(resetPasswordModel.Password);
            var user = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.Account == resetPasswordModel.Account && u.Password == resetPasswordModel.OldPassword);
            if (user == null)
            {
                return OperateResult.Failed("用户名或密码错误");
            }
            if(resetPasswordModel.Password != resetPasswordModel.ConfirmPassword)
            {
                return OperateResult.Failed("两次输入的密码不一致");
            }
            user.Password = Md5Helper.GetMd5Hash(resetPasswordModel.Password);
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            return OperateResult.Successed();
        }

    }
}
