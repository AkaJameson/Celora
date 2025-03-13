using Quartz;
using Quartz.Impl;

namespace Si.CoreHub.Scheduling
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduler _scheduler = new StdSchedulerFactory().GetScheduler().Result;

        public async Task ScheduleJob<TJob>(ScheduleOptions options, SimpleScheduleConfig simpleScheduleConfig = null) where TJob : IJob
        {
            // 创建任务，设置唯一 JobKey 与描述
            var job = JobBuilder.Create<TJob>()
                                .WithIdentity(options.JobKey, options.GroupName)
                                .WithDescription(options.Description)
                                .Build();

            ITrigger trigger;

            // 如果传入了 Cron 表达式，则优先使用 CronTrigger
            if (!string.IsNullOrEmpty(options.CronExpression))
            {
                var triggerBuilder = TriggerBuilder.Create()
                                                   .WithIdentity(options.JobKey + "_trigger", options.GroupName)
                                                   .WithCronSchedule(options.CronExpression);

                triggerBuilder = triggerBuilder.StartAt(options.DateTimeOffset);

                trigger = triggerBuilder.Build();
            }
            // 否则使用简单调度配置
            else if (simpleScheduleConfig != null)
            {
                var triggerBuilder = TriggerBuilder.Create()
                                                   .WithIdentity(options.JobKey + "_trigger", options.GroupName);

                triggerBuilder = triggerBuilder.StartAt(options.DateTimeOffset);

                trigger = triggerBuilder.WithSimpleSchedule(x =>
                {
                    if (simpleScheduleConfig.RepeatCount == -1)
                    {
                        x.WithIntervalInSeconds(simpleScheduleConfig.IntervalInSeconds)
                         .RepeatForever();
                    }
                    else
                    {
                        x.WithIntervalInSeconds(simpleScheduleConfig.IntervalInSeconds)
                         .WithRepeatCount(simpleScheduleConfig.RepeatCount);
                    }
                }).Build();
            }
            
            else
            {
                throw new ArgumentException("必须提供 CronExpression 或 SimpleScheduleConfig 其中一种配置。");
            }

            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task<bool> RemoveJob(string jobKey, string groupName = "DEFAULT")
        {
            return await _scheduler.DeleteJob(new JobKey(jobKey, groupName));
        }
    }
}
