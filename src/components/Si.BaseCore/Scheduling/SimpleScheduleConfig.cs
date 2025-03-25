namespace Si.CoreHub.Scheduling
{
    public class SimpleScheduleConfig
    {
        /// <summary>
        /// 间隔（秒）
        /// </summary>
        public int IntervalInSeconds { get; set; } = 60;

        /// <summary>
        /// 重复次数（-1表示无限次）
        /// </summary>
        public int RepeatCount { get; set; } = -1;
    }
}
