namespace Si.EntityFramework.IdentityServer.Configuration
{
    /// <summary>
    /// 密码设置
    /// </summary>
    public class PasswordSettings
    {
        /// <summary>
        /// 最小长度
        /// </summary>
        public int MinLength { get; set; } = 6;

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; set; } = 30;

        /// <summary>
        /// 是否需要数字
        /// </summary>
        public bool RequireDigit { get; set; } = true;

        /// <summary>
        /// 是否需要小写字母
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// 是否需要大写字母
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// 是否需要特殊字符
        /// </summary>
        public bool RequireNonAlphanumeric { get; set; } = true;

        /// <summary>
        /// 密码历史记录数量
        /// </summary>
        public int PasswordHistorySize { get; set; } = 5;
    }
}
