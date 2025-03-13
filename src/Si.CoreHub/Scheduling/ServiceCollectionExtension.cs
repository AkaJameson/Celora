using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;

namespace Si.CoreHub.Scheduling
{
    public static class ServiceCollectionExtension
    {
        public static void AddScheduleService(this IServiceCollection services)
        {
            services.AddSingleton<IScheduleService, ScheduleService>();
        }
    }
}
