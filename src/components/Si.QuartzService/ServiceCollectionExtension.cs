using Microsoft.Extensions.DependencyInjection;

namespace Si.QuartzService
{
    public static class ServiceCollectionExtension
    {
        public static void AddScheduleService(this IServiceCollection services)
        {
            services.AddSingleton<IScheduleService, ScheduleService>();
        }
    }
}
