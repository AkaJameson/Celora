using Si.EntityFramework.Extension.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Si.EntityFramework.Extension.Core.Utils;
namespace Si.EntityFramework.Extension.Data.Configurations
{
    /// <summary>
    /// 数据库配置选项
    /// </summary>
    public class DbOptions
    {

        /// <summary>
        /// 是否启用全局查询过滤器
        /// </summary>
        public bool EnableGlobalFilters { get; set; } = true;

        /// <summary>
        /// 是否启用软删除
        /// </summary>
        public bool EnableSoftDelete { get; set; } = true;

        /// <summary>
        /// 是否自动设置审计信息
        /// </summary>
        public bool EnableAuditTracking { get; set; } = true;
        /// <summary>
        /// 是否启用多租户
        /// </summary>
        public bool EnableMultiTenant { get; set; } = false;

        /// <summary>
        /// 忽略多租户过滤的实体类型
        /// </summary> 
        public List<Type> IgnoredMultiTenantTypes { get; set; } = new();

        /// <summary>
        /// 获取当前用户ID的函数
        /// </summary>
        public Func<IUser> CurrentUserIdProvider { get; set; } = () => null;

        /// <summary>
        /// 获取当前租户ID的函数
        /// </summary>
        public Func<ITenant> CurrentTenantIdProvider { get; set; } = () => null;
        /// <summary>
        /// 是否启用雪花ID
        /// </summary>
        public bool EnableSnowflakeId { get; set; } = false;
        /// <summary>
        /// 短id生成器
        /// </summary>
        public ShortIdGenerator ShortIdGenerator { get; set; }
        /// <summary>
        /// 长id生成器
        /// </summary>
        public LongIdGenerator LongIdGenerator { get; set; }
   
    }
}
