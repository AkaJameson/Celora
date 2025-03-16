using Microsoft.Extensions.DependencyInjection;

namespace Si.Utilites.Captcha
{
    public static class CaptchaExtension
    {
        public static IServiceCollection AddCaptcha(
            this IServiceCollection services,
            Action<CaptchaOptions> configure = null)
        {
            var options = new CaptchaOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);
            services.AddMemoryCache();
            services.AddScoped<CaptchaGenerator>();
            services.AddScoped<ICaptchaService, CaptchaService>();

            return services;
        }
    }
}
