using Microsoft.EntityFrameworkCore;
using Si.EntityFrame.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Services;

namespace Si.EntityFramework.IdentityServer.ServicesImpl
{
    /// <summary>
    /// 角色权限服务实现
    /// </summary>
    public class RolePermissionService : IRolePermissionService
    {
        private readonly DbContext _dbContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RolePermissionService(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #region 角色管理

        /// <summary>
        /// 创建角色
        /// </summary>
        public async Task<Role> CreateRoleAsync(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("角色名称不能为空", nameof(name));
            }

            // 检查角色是否已存在
            if (await _dbContext.Set<Role>().AnyAsync(r => r.Name == name))
            {
                throw new InvalidOperationException($"角色 '{name}' 已存在");
            }

            var role = new Role
            {
                Name = name,
                Description = description,
                CreateTime = DateTime.UtcNow
            };

            _dbContext.Set<Role>().Add(role);
            await _dbContext.SaveChangesAsync();

            return role;
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        public async Task<bool> UpdateRoleAsync(int roleId, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("角色名称不能为空", nameof(name));
            }

            var role = await _dbContext.Set<Role>().FindAsync(roleId);
            if (role == null)
            {
                return false;
            }

            // 检查名称是否已被其他角色使用
            if (await _dbContext.Set<Role>().AnyAsync(r => r.Name == name && r.Id != roleId))
            {
                throw new InvalidOperationException($"角色名称 '{name}' 已被使用");
            }

            role.Name = name;
            role.Description = description;
            _dbContext.Update(role);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var role = await _dbContext.Set<Role>()
                .Include(r => r.Users)
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                return false;
            }

            // 移除与用户的关联
            if (role.Users != null && role.Users.Any())
            {
                role.Users.Clear();
            }

            // 移除与权限的关联
            if (role.Permissions != null && role.Permissions.Any())
            {
                role.Permissions.Clear();
            }

            _dbContext.Set<Role>().Remove(role);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 获取角色
        /// </summary>
        public async Task<Role> GetRoleAsync(int roleId)
        {
            return await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        /// <summary>
        /// 根据名称获取角色
        /// </summary>
        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return null;
            }

            return await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .ToListAsync();
        }

        /// <summary>
        /// 分页获取角色
        /// </summary>
        public async Task<IEnumerable<Role>> GetRolesAsync(int pageIndex = 0, int pageSize = 10)
        {
            return await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .OrderBy(r => r.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 获取角色总数
        /// </summary>
        public async Task<int> GetRoleCountAsync()
        {
            return await _dbContext.Set<Role>().CountAsync();
        }

        #endregion

        #region 权限管理

        /// <summary>
        /// 创建权限
        /// </summary>
        public async Task<Permission> CreatePermissionAsync(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("权限名称不能为空", nameof(name));
            }

            // 检查权限是否已存在
            if (await _dbContext.Set<Permission>().AnyAsync(p => p.PermessionName == name))
            {
                throw new InvalidOperationException($"权限 '{name}' 已存在");
            }

            var permission = new Permission
            {
                PermessionName = name,
                Description = description,
                CreateTime = DateTime.UtcNow
            };

            _dbContext.Set<Permission>().Add(permission);
            await _dbContext.SaveChangesAsync();

            return permission;
        }

        /// <summary>
        /// 更新权限
        /// </summary>
        public async Task<bool> UpdatePermissionAsync(int permissionId, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("权限名称不能为空", nameof(name));
            }

            var permission = await _dbContext.Set<Permission>().FindAsync(permissionId);
            if (permission == null)
            {
                return false;
            }

            // 检查名称是否已被其他权限使用
            if (await _dbContext.Set<Permission>().AnyAsync(p => p.PermessionName == name && p.Id != permissionId))
            {
                throw new InvalidOperationException($"权限名称 '{name}' 已被使用");
            }

            permission.PermessionName = name;
            permission.Description = description;

            _dbContext.Update(permission);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        public async Task<bool> DeletePermissionAsync(int permissionId)
        {
            var permission = await _dbContext.Set<Permission>()
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Id == permissionId);

            if (permission == null)
            {
                return false;
            }

            // 移除与角色的关联
            if (permission.Roles != null && permission.Roles.Any())
            {
                permission.Roles.Clear();
            }

            _dbContext.Set<Permission>().Remove(permission);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 获取权限
        /// </summary>
        public async Task<Permission> GetPermissionAsync(int permissionId)
        {
            return await _dbContext.Set<Permission>()
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Id == permissionId);
        }

        /// <summary>
        /// 根据名称获取权限
        /// </summary>
        public async Task<Permission> GetPermissionByNameAsync(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                return null;
            }

            return await _dbContext.Set<Permission>()
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.PermessionName == permissionName);
        }

        /// <summary>
        /// 获取所有权限
        /// </summary>
        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return await _dbContext.Set<Permission>()
                .Include(p => p.Roles)
                .ToListAsync();
        }

        /// <summary>
        /// 分页获取权限
        /// </summary>
        public async Task<IEnumerable<Permission>> GetPermissionsAsync(int pageIndex = 0, int pageSize = 10)
        {
            return await _dbContext.Set<Permission>()
                .Include(p => p.Roles)
                .OrderBy(p => p.PermessionName)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 获取权限总数
        /// </summary>
        public async Task<int> GetPermissionCountAsync()
        {
            return await _dbContext.Set<Permission>().CountAsync();
        }

        #endregion

        #region 角色权限关联

        /// <summary>
        /// 给角色分配权限
        /// </summary>
        public async Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
        {
            var role = await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                return false;
            }

            // 获取权限
            var permissions = await _dbContext.Set<Permission>()
                .Where(p => permissionIds.Contains(p.Id))
                .ToListAsync();

            // 更新角色权限
            role.Permissions.Clear();
            foreach (var permission in permissions)
            {
                role.Permissions.Add(permission);
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 从角色移除权限
        /// </summary>
        public async Task<bool> RemovePermissionsFromRoleAsync(int roleId, List<int> permissionIds)
        {
            var role = await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                return false;
            }

            // 移除指定权限
            foreach (var permissionId in permissionIds)
            {
                var permission = role.Permissions.FirstOrDefault(p => p.Id == permissionId);
                if (permission != null)
                {
                    role.Permissions.Remove(permission);
                }
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 获取角色的所有权限
        /// </summary>
        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId)
        {
            var role = await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            return role?.Permissions ?? Enumerable.Empty<Permission>();
        }

        /// <summary>
        /// 获取拥有特定权限的所有角色
        /// </summary>
        public async Task<IEnumerable<Role>> GetRolesForPermissionAsync(int permissionId)
        {
            var permission = await _dbContext.Set<Permission>()
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Id == permissionId);

            return permission?.Roles ?? Enumerable.Empty<Role>();
        }

        /// <summary>
        /// 判断角色是否拥有特定权限
        /// </summary>
        public async Task<bool> RoleHasPermissionAsync(int roleId, string permissionName)
        {
            return await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .AnyAsync(r => r.Id == roleId && r.Permissions.Any(p => p.PermessionName == permissionName));
        }

        #endregion

        #region 用户角色关联

        /// <summary>
        /// 获取用户的所有角色
        /// </summary>
        public async Task<IEnumerable<Role>> GetRolesForUserAsync(int userId)
        {
            var user = await _dbContext.Set<User>()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Roles ?? Enumerable.Empty<Role>();
        }

        /// <summary>
        /// 获取用户的所有权限
        /// </summary>
        public async Task<IEnumerable<Permission>> GetPermissionsForUserAsync(int userId)
        {
            var user = await _dbContext.Set<User>()
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Roles == null)
            {
                return Enumerable.Empty<Permission>();
            }

            // 获取用户所有角色的所有权限
            return user.Roles
                .SelectMany(r => r.Permissions)
                .Distinct();
        }

        /// <summary>
        /// 判断用户是否拥有特定角色
        /// </summary>
        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            return await _dbContext.Set<User>()
                .Include(u => u.Roles)
                .AnyAsync(u => u.Id == userId && u.Roles.Any(r => r.Name == roleName));
        }

        /// <summary>
        /// 判断用户是否拥有特定权限
        /// </summary>
        public async Task<bool> UserHasPermissionAsync(int userId, string permissionName)
        {
            return await _dbContext.Set<User>()
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .AnyAsync(u => u.Id == userId && u.Roles.Any(r => r.Permissions.Any(p => p.PermessionName == permissionName)));
        }

        #endregion
    }

}
