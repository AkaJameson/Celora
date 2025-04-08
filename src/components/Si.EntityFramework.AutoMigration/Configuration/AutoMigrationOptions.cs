namespace Si.EntityFramework.AutoMigration.Configuration
{
    public class AutoMigrationOptions
    {
        /// <summary>
        /// 是否使用自动迁移记录
        /// </summary>
        public bool UseAutoMigrationRecord { get; set; } = true;
        /// <summary>
        /// 备份数据库
        /// </summary>
        public bool BackupDatabase { get; set; } = false;
        /// <summary>
        /// 验证数据库连接
        /// </summary>
        public bool ValidateConnection { get; set; } = true;
        /// <summary>
        /// 生成迁移脚本
        /// </summary>
        public bool GenerateScript { get; set; } = false;
        /// <summary>
        /// 允许删除列
        /// </summary>
        public bool AllowDropColumn { get; set; } = false;
        /// <summary>
        /// 允许删除表
        /// </summary>
        public bool AllowDropTable { get; set; } = false;
    }
}
