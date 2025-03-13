namespace Si.EntityFrame.IdentityServer.Entitys
{
    public class Permission
    {
        /// <summary>
        /// 权限Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 权限名称
        /// </summary>
        public string PermessionName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public virtual IList<Role> Roles { get; set; }
    }
   
}
