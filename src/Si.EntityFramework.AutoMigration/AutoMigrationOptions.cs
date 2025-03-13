namespace Si.EntityFramework.AutoMigration
{
    /// <summary>
    /// 自动迁移选项
    /// </summary>
    public class AutoMigrationOptions
    {
        /// <summary>
        /// 是否在出错时抛出异常
        /// </summary>
        public bool ThrowOnError { get; set; } = false;
        
        /// <summary>
        /// 是否显示详细错误信息
        /// </summary>
        public bool DetailedErrors { get; set; } = true;
        
        /// <summary>
        /// 是否跟踪迁移历史
        /// </summary>
        public bool TrackHistory { get; set; } = true;
        
        /// <summary>
        /// 是否允许删除列
        /// </summary>
        public bool AllowDropColumn { get; set; } = false;
        
        /// <summary>
        /// 是否允许删除表
        /// </summary>
        public bool AllowDropTable { get; set; } = false;
        
        /// <summary>
        /// 是否仅生成脚本而不执行
        /// </summary>
        public bool ScriptOnly { get; set; } = false;

        /// <summary>
        /// 是否验证数据库连接
        /// </summary>
        public bool ValidateConnection { get; set; } = true;

        /// <summary>
        /// 是否备份数据库
        /// </summary>
        public bool BackupDatabase { get; set; } = true;
        /// <summary>
        /// 迁移历史表名
        /// </summary>
        public string HistoryTableName { get; set; } = "__AutoMigrationHistory";
    }
} 