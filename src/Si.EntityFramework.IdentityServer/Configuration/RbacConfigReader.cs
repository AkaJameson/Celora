using Microsoft.EntityFrameworkCore;
using Si.EntityFrame.IdentityServer.Entitys;
using System.Xml.Linq;

namespace Si.EntityFramework.IdentityServer.Configuration
{
    /// <summary>
    /// RBAC配置读取器
    /// </summary>
    public class RbacConfigReader
    {
        private readonly DbContext _dbContext;

        public RbacConfigReader(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 从XML文件初始化RBAC配置
        /// </summary>
        /// <param name="xmlFilePath">XML文件路径</param>
        /// <returns>初始化是否成功</returns>
        public async Task<bool> InitializeFromXmlAsync(string xmlFilePath)
        {
            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException($"找不到RBAC配置文件: {xmlFilePath}");
            }

            try
            {
                // 读取XML文件
                var xDoc = XDocument.Load(xmlFilePath);
                var rbacElement = xDoc.Element("Rbac");
                if (rbacElement == null)
                {
                    throw new InvalidOperationException("XML文件格式不正确，缺少Rbac根元素");
                }

                // 清空现有权限和角色
                await ClearExistingDataAsync();

                // 读取并创建权限
                var permissionsDict = await CreatePermissionsAsync(rbacElement);

                // 读取并创建角色，同时关联权限
                await CreateRolesAsync(rbacElement, permissionsDict);

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"初始化RBAC配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清空现有的权限和角色数据
        /// </summary>
        private async Task ClearExistingDataAsync()
        {
            // 删除角色-权限关联
            var roles = await _dbContext.Set<Role>()
                .Include(r => r.Permissions)
                .ToListAsync();

            foreach (var role in roles)
            {
                if (role.Permissions != null)
                {
                    role.Permissions.Clear();
                }
            }
            await _dbContext.SaveChangesAsync();

            // 删除角色和权限
            _dbContext.Set<Role>().RemoveRange(roles);
            var permissions = await _dbContext.Set<Permission>().ToListAsync();
            _dbContext.Set<Permission>().RemoveRange(permissions);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 创建权限
        /// </summary>
        private async Task<Dictionary<int, Permission>> CreatePermissionsAsync(XElement rbacElement)
        {
            var result = new Dictionary<int, Permission>();
            var permissionsElement = rbacElement.Element("Permissions");
            if (permissionsElement == null)
            {
                return result;
            }

            foreach (var permElement in permissionsElement.Elements("Permission"))
            {
                // 获取ID
                var idElement = permElement.Element("Id");
                if (idElement == null || !int.TryParse(idElement.Value, out int id))
                {
                    continue;
                }

                // 获取名称
                var nameElement = permElement.Element("n");
                if (nameElement == null || string.IsNullOrWhiteSpace(nameElement.Value))
                {
                    continue;
                }

                // 获取描述
                var descElement = permElement.Element("Description");
                string description = descElement?.Value ?? string.Empty;

                // 创建权限
                var permission = new Permission
                {
                    Id = id,
                    PermessionName = nameElement.Value,
                    Description = description,
                    Roles = new List<Role>()
                };

                _dbContext.Set<Permission>().Add(permission);
                result.Add(id, permission);
            }

            await _dbContext.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// 创建角色并关联权限
        /// </summary>
        private async Task CreateRolesAsync(XElement rbacElement, Dictionary<int, Permission> permissionsDict)
        {
            var rolesElement = rbacElement.Element("Roles");
            if (rolesElement == null)
            {
                return;
            }

            foreach (var roleElement in rolesElement.Elements("Role"))
            {
                // 获取ID
                var idElement = roleElement.Element("Id");
                if (idElement == null || !int.TryParse(idElement.Value, out int id))
                {
                    continue;
                }

                // 获取名称
                var nameElement = roleElement.Element("n");
                if (nameElement == null || string.IsNullOrWhiteSpace(nameElement.Value))
                {
                    continue;
                }

                // 获取描述
                var descElement = roleElement.Element("Description");
                string description = descElement?.Value ?? string.Empty;

                // 创建角色
                var role = new Role
                {
                    Id = id,
                    Name = nameElement.Value,
                    Description = description,
                    Permissions = new List<Permission>(),
                    Users = new List<User>()
                };

                // 关联权限
                var permissionsElement = roleElement.Element("Permissions");
                if (permissionsElement != null)
                {
                    foreach (var permIdElement in permissionsElement.Elements("PermissionId"))
                    {
                        if (int.TryParse(permIdElement.Value, out int permId) &&
                            permissionsDict.TryGetValue(permId, out var permission))
                        {
                            role.Permissions.Add(permission);
                        }
                    }
                }

                _dbContext.Set<Role>().Add(role);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
