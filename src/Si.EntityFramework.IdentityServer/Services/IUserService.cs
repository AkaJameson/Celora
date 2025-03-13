using Si.EntityFrame.IdentityServer.Entitys;
using Si.EntityFrame.IdentityServer.Models;
using Si.EntityFramework.IdentityServer.Models;

namespace Si.EntityFramework.IdentityServer.Services
{
    /// <summary>
    /// 用户服务接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        Task<LoginResult> LoginAsync(string username, string password, bool useCookie = true);

        /// <summary>
        /// 用户注册
        /// </summary>
        Task<RegisterResult> RegisterAsync(RegisterModel model);

        /// <summary>
        /// 刷新令牌
        /// </summary>
        Task<RefreshTokenResult> RefreshTokenAsync(string accessToken, string refreshToken);

        /// <summary>
        /// 用户登出
        /// </summary>
        void Logout(bool removeCookie = true);

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        Task<User> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        Task<User> GetUserByUsernameAsync(string username);

        /// <summary>
        /// 更新用户
        /// </summary>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// 修改密码
        /// </summary>
        Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        /// <summary>
        /// 重置密码
        /// </summary>
        Task<ResetPasswordResult> ResetPasswordAsync(string username, string newPassword);

        /// <summary>
        /// 禁用用户
        /// </summary>
        Task<bool> DisableUserAsync(Guid userId);

        /// <summary>
        /// 启用用户
        /// </summary>
        Task<bool> EnableUserAsync(Guid userId);

        /// <summary>
        /// 获取用户列表
        /// </summary>
        Task<IEnumerable<User>> GetUsersAsync(int pageIndex = 0, int pageSize = 10);

        /// <summary>
        /// 获取用户总数
        /// </summary>
        Task<int> GetUserCountAsync();

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        Task<bool> AssignRolesToUserAsync(Guid userId, List<Guid> roleIds);
    }
}
