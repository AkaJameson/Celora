using Microsoft.EntityFrameworkCore;
using Si.EntityFrame.IdentityServer.Entitys;
using Si.EntityFrame.IdentityServer.Models;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Models;
using Si.EntityFramework.IdentityServer.Services;
using System.Text.RegularExpressions;

namespace Si.EntityFramework.IdentityServer.ServicesImpl
{
    /// <summary>
    /// 用户服务，处理用户认证、授权和管理
    /// </summary>
    public class UserService : IUserService
    {
        private readonly DbContext _dbContext;
        private readonly JwtManager _jwtManager;
        private readonly CookieManager _cookieManager;
        private readonly StringHasher _passwordHasher;
        private readonly IRolePermissionService _rolePermissionService;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UserService(
            DbContext dbContext,
            JwtManager jwtManager,
            CookieManager cookieManager,
            StringHasher passwordHasher,
            IRolePermissionService rolePermissionService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _jwtManager = jwtManager ?? throw new ArgumentNullException(nameof(jwtManager));
            _cookieManager = cookieManager ?? throw new ArgumentNullException(nameof(cookieManager));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _rolePermissionService = rolePermissionService ?? throw new ArgumentNullException(nameof(rolePermissionService));
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        public async Task<LoginResult> LoginAsync(string username, string password, bool useCookie = true)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new LoginResult { Success = false, Message = "用户名或密码不能为空" };
            }

            // 查找用户
            var user = await _dbContext.Set<User>()
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Account == username);

            // 用户不存在
            if (user == null)
            {
                return new LoginResult { Success = false, Message = "用户不存在" };
            }

            // 验证密码
            if (!_passwordHasher.VerifyString(password, user.Password))
            {
                return new LoginResult { Success = false, Message = "密码不正确" };
            }

            // 检查用户状态
            if (user.Lock)
            {
                return new LoginResult { Success = false, Message = "用户被禁用" };
            }

            await _dbContext.SaveChangesAsync();

            // 获取用户角色和权限
            var roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>();
            var permissions = user.Roles?
                .SelectMany(r => r.Permissions)
                .Select(p => p.PermessionName)
                .Distinct()
                .ToList() ?? new List<string>();

            // 生成JWT令牌
            var tokenInfo = _jwtManager.GenerateToken(user, roles, permissions);

            // 如果使用Cookie，则设置Cookie
            if (useCookie)
            {
                _cookieManager.SetAuthCookie(user, user.Account, roles, permissions);
            }

            return new LoginResult
            {
                Success = true,
                Message = "登录成功",
                UserId = user.Id,
                Username = user.Account,
                Token = tokenInfo.AccessToken,
                RefreshToken = tokenInfo.RefreshToken,
                Roles = roles,
                Permissions = permissions
            };
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        public async Task<RegisterResult> RegisterAsync(RegisterModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // 验证用户数据
            var validationResult = ValidateRegisterModel(model);
            if (!validationResult.IsValid)
            {
                return new RegisterResult { Success = false, Message = validationResult.Message };
            }

            // 检查用户名是否已存在
            if (await _dbContext.Set<User>().AnyAsync(u => u.Account == model.Username))
            {
                return new RegisterResult { Success = false, Message = "用户名已存在" };
            }

            // 检查邮箱是否已存在
            if (!string.IsNullOrEmpty(model.Email) && await _dbContext.Set<User>().AnyAsync(u => u.Email == model.Email))
            {
                return new RegisterResult { Success = false, Message = "邮箱已被使用" };
            }

            // 创建新用户
            var user = new User
            {
                Account = model.Username,
                Password = _passwordHasher.HashPassword(model.Password),
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = _passwordHasher.GenerateSecurityStamp(),
                IsActive = true,
                CreationTime = DateTime.Now,
                LastModificationTime = DateTime.Now
            };

            // 添加默认角色
            if (!string.IsNullOrEmpty(model.DefaultRoleName))
            {
                var defaultRole = await _dbContext.Set<Role>().FirstOrDefaultAsync(r => r.Name == model.DefaultRoleName);
                if (defaultRole != null)
                {
                    user.Roles = new List<Role> { defaultRole };
                }
            }

            // 保存用户
            _dbContext.Set<User>().Add(user);
            await _dbContext.SaveChangesAsync();

            return new RegisterResult
            {
                Success = true,
                Message = "注册成功",
                UserId = user.Id
            };
        }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public async Task<RefreshTokenResult> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return new RefreshTokenResult { Success = false, Message = "令牌不能为空" };
            }

            try
            {
                // 验证并刷新令牌
                var tokenInfo = _jwtManager.RefreshToken(accessToken, refreshToken);
                if (tokenInfo == null)
                {
                    return new RefreshTokenResult { Success = false, Message = "无效的令牌" };
                }

                return new RefreshTokenResult
                {
                    Success = true,
                    Message = "刷新令牌成功",
                    AccessToken = tokenInfo.AccessToken,
                    RefreshToken = tokenInfo.RefreshToken
                };
            }
            catch (Exception ex)
            {
                return new RefreshTokenResult { Success = false, Message = $"刷新令牌失败：{ex.Message}" };
            }
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        public void Logout(bool removeCookie = true)
        {
            // 如果使用Cookie，则移除Cookie
            if (removeCookie)
            {
                _cookieManager.RemoveAuthCookie();
            }
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await _dbContext.Set<User>()
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return await _dbContext.Set<User>()
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Account == username);
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                user.LastModificationTime = DateTime.Now;
                _dbContext.Update(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public async Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return new ChangePasswordResult { Success = false, Message = "密码不能为空" };
            }

            // 获取用户
            var user = await _dbContext.Set<User>().FindAsync(userId);
            if (user == null)
            {
                return new ChangePasswordResult { Success = false, Message = "用户不存在" };
            }

            // 验证当前密码
            if (!_passwordHasher.VerifyPassword(currentPassword, user.Password))
            {
                return new ChangePasswordResult { Success = false, Message = "当前密码不正确" };
            }

            // 验证新密码
            var passwordValidationResult = ValidatePassword(newPassword);
            if (!passwordValidationResult.IsValid)
            {
                return new ChangePasswordResult { Success = false, Message = passwordValidationResult.Message };
            }

            // 检查密码历史
            if (_passwordHasher.IsPasswordInHistory(newPassword, user.PasswordHistory))
            {
                return new ChangePasswordResult { Success = false, Message = "新密码不能与历史密码相同" };
            }

            // 更新密码
            user.PasswordHistory = _passwordHasher.UpdatePasswordHistory(user.Password, user.PasswordHistory);
            user.Password = _passwordHasher.HashPassword(newPassword);
            user.SecurityStamp = _passwordHasher.GenerateSecurityStamp();
            user.LastModificationTime = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return new ChangePasswordResult { Success = true, Message = "密码修改成功" };
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        public async Task<ResetPasswordResult> ResetPasswordAsync(string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
            {
                return new ResetPasswordResult { Success = false, Message = "用户名或密码不能为空" };
            }

            // 获取用户
            var user = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.Account == username);
            if (user == null)
            {
                return new ResetPasswordResult { Success = false, Message = "用户不存在" };
            }

            // 验证新密码
            var passwordValidationResult = ValidatePassword(newPassword);
            if (!passwordValidationResult.IsValid)
            {
                return new ResetPasswordResult { Success = false, Message = passwordValidationResult.Message };
            }

            // 更新密码
            user.PasswordHistory = _passwordHasher.UpdatePasswordHistory(user.Password, user.PasswordHistory);
            user.Password = _passwordHasher.HashPassword(newPassword);
            user.SecurityStamp = _passwordHasher.GenerateSecurityStamp();
            user.LastModificationTime = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return new ResetPasswordResult { Success = true, Message = "密码重置成功" };
        }

        /// <summary>
        /// 禁用用户
        /// </summary>
        public async Task<bool> DisableUserAsync(Guid userId)
        {
            var user = await _dbContext.Set<User>().FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            user.LastModificationTime = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 启用用户
        /// </summary>
        public async Task<bool> EnableUserAsync(Guid userId)
        {
            var user = await _dbContext.Set<User>().FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = true;
            user.LastModificationTime = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        public async Task<IEnumerable<User>> GetUsersAsync(int pageIndex = 0, int pageSize = 10)
        {
            return await _dbContext.Set<User>()
                .Include(u => u.Roles)
                .OrderBy(u => u.Account)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 获取用户总数
        /// </summary>
        public async Task<int> GetUserCountAsync()
        {
            return await _dbContext.Set<User>().CountAsync();
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        public async Task<bool> AssignRolesToUserAsync(Guid userId, List<Guid> roleIds)
        {
            var user = await _dbContext.Set<User>()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            // 获取角色
            var roles = await _dbContext.Set<Role>()
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync();

            // 更新用户角色
            user.Roles.Clear();
            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }

            user.LastModificationTime = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #region 私有方法

        /// <summary>
        /// 验证注册模型
        /// </summary>
        private ValidationResult ValidateRegisterModel(RegisterModel model)
        {
            // 验证用户名
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                return new ValidationResult { IsValid = false, Message = "用户名不能为空" };
            }

            if (model.Username.Length < 4 || model.Username.Length > 20)
            {
                return new ValidationResult { IsValid = false, Message = "用户名长度必须在4-20个字符之间" };
            }

            // 验证密码
            var passwordValidationResult = ValidatePassword(model.Password);
            if (!passwordValidationResult.IsValid)
            {
                return passwordValidationResult;
            }

            // 验证邮箱
            if (!string.IsNullOrWhiteSpace(model.Email) && !IsValidEmail(model.Email))
            {
                return new ValidationResult { IsValid = false, Message = "邮箱格式不正确" };
            }

            // 验证手机号
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber) && !IsValidPhoneNumber(model.PhoneNumber))
            {
                return new ValidationResult { IsValid = false, Message = "手机号格式不正确" };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        private ValidationResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult { IsValid = false, Message = "密码不能为空" };
            }

            if (password.Length < 6 || password.Length > 30)
            {
                return new ValidationResult { IsValid = false, Message = "密码长度必须在6-30个字符之间" };
            }

            // 密码复杂度要求：包含数字、字母和特殊字符
            var hasNumber = new Regex(@"[0-9]+");
            var hasLetter = new Regex(@"[a-zA-Z]+");
            var hasSpecialChar = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':\\|,.<>\/?]+");

            if (!hasNumber.IsMatch(password) || !hasLetter.IsMatch(password) || !hasSpecialChar.IsMatch(password))
            {
                return new ValidationResult { IsValid = false, Message = "密码必须包含数字、字母和特殊字符" };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 验证邮箱
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证手机号
        /// </summary>
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            // 中国手机号验证
            return Regex.IsMatch(phoneNumber, @"^1[3-9]\d{9}$");
        }

        #endregion
    }

}
