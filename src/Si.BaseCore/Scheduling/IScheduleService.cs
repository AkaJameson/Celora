using Quartz;

namespace Si.CoreHub.Scheduling
{
    public interface IScheduleService
    {
        /// <summary>
        /// 根据配置注册任务
        /// </summary>
        /// <typeparam name="TJob">任务类型，需要实现 IJob</typeparam>
        /// <param name="options">任务配置</param>
        /// <param name="simpleScheduleConfig">
        /// 如果没有提供 Cron 表达式，则可以传入简单调度配置；
        /// 若两者都提供，则优先使用 Cron 表达式
        /// </param>
        Task ScheduleJob<TJob>(ScheduleOptions options, SimpleScheduleConfig simpleScheduleConfig = null) where TJob : IJob;

        /// <summary>
        /// 删除指定任务
        /// </summary>
        Task<bool> RemoveJob(string jobKey, string groupName = "DEFAULT");
    }
}
