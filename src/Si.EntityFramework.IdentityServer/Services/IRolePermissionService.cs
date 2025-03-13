using Si.EntityFrame.IdentityServer.Entitys;

namespace Si.EntityFramework.IdentityServer.Services
{
    /// <summary>
    /// 角色权限服务接口
    /// </summary>
    public interface IRolePermissionService
    {
        #region 角色管理

        /// <summary>
        /// 创建角色
        /// </summary>
        Task<Role> CreateRoleAsync(string name, string description = null);

        /// <summary>
        /// 更新角色
        /// </summary>
        Task<bool> UpdateRoleAsync(int roleId, string name, string description);

        /// <summary>
        /// 删除角色
        /// </summary>
        Task<bool> DeleteRoleAsync(int roleId);

        /// <summary>
        /// 获取角色
        /// </summary>
        Task<Role> GetRoleAsync(int roleId);

        /// <summary>
        /// 根据名称获取角色
        /// </summary>
        Task<Role> GetRoleByNameAsync(string roleName);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        Task<IEnumerable<Role>> GetAllRolesAsync();

        /// <summary>
        /// 分页获取角色
        /// </summary>
        Task<IEnumerable<Role>> GetRolesAsync(int pageIndex = 0, int pageSize = 10);

        /// <summary>
        /// 获取角色总数
        /// </summary>
        Task<int> GetRoleCountAsync();

        #endregion

        #region 权限管理

        /// <summary>
        /// 创建权限
        /// </summary>
        Task<Permission> CreatePermissionAsync(string name, string description = null);

        /// <summary>
        /// 更新权限
        /// </summary>
        Task<bool> UpdatePermissionAsync(int permissionId, string name, string description);

        /// <summary>
        /// 删除权限
        /// </summary>
        Task<bool> DeletePermissionAsync(int permissionId);

        /// <summary>
        /// 获取权限
        /// </summary>
        Task<Permission> GetPermissionAsync(int permissionId);

        /// <summary>
        /// 根据名称获取权限
        /// </summary>
        Task<Permission> GetPermissionByNameAsync(string permissionName);

        /// <summary>
        /// 获取所有权限
        /// </summary>
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();

        /// <summary>
        /// 分页获取权限
        /// </summary>
        Task<IEnumerable<Permission>> GetPermissionsAsync(int pageIndex = 0, int pageSize = 10);

        /// <summary>
        /// 获取权限总数
        /// </summary>
        Task<int> GetPermissionCountAsync();

        #endregion

        #region 角色权限关联

        /// <summary>
        /// 给角色分配权限
        /// </summary>
        Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds);

        /// <summary>
        /// 从角色移除权限
        /// </summary>
        Task<bool> RemovePermissionsFromRoleAsync(int roleId, List<int> permissionIds);

        /// <summary>
        /// 获取角色的所有权限
        /// </summary>
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId);

        /// <summary>
        /// 获取拥有特定权限的所有角色
        /// </summary>
        Task<IEnumerable<Role>> GetRolesForPermissionAsync(int permissionId);

        /// <summary>
        /// 判断角色是否拥有特定权限
        /// </summary>
        Task<bool> RoleHasPermissionAsync(int roleId, string permissionName);

        #endregion

        #region 用户角色关联

        /// <summary>
        /// 获取用户的所有角色
        /// </summary>
        Task<IEnumerable<Role>> GetRolesForUserAsync(int userId);

        /// <summary>
        /// 获取用户的所有权限
        /// </summary>
        Task<IEnumerable<Permission>> GetPermissionsForUserAsync(int userId);

        /// <summary>
        /// 判断用户是否拥有特定角色
        /// </summary>
        Task<bool> UserHasRoleAsync(int userId, string roleName);

        /// <summary>
        /// 判断用户是否拥有特定权限
        /// </summary>
        Task<bool> UserHasPermissionAsync(int userId, string permissionName);

        #endregion
    }
}
