using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Si.Modules.Swagger
{
    /// <summary>
    /// API分组特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiGroupAttribute : Attribute, IApiDescriptionGroupNameProvider
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="groupName">分组名称</param>
        public ApiGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}
