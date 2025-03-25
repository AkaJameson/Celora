namespace Si.CoreHub.Scheduling
{
    public class ScheduleOptions
    {
        /// <summary>
        /// 作业键名（唯一标识）
        /// </summary>
        public string JobKey { get; set; }

        /// <summary>
        /// 作业组名
        /// </summary>
        public string GroupName { get; set; } = "DEFAULT";

        /// <summary>
        /// 作业描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// 是否立即启动
        /// </summary>
        public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;
    }
}
