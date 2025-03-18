using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Si.CoreHub.Logging;
using System.Net;

namespace Si.CoreHub.Utility
{
    public static class KestrelConfig
    {
        /// <summary>
        /// 配置Kestrel服务器
        /// </summary>
        /// <param name="builder">Web应用程序构建器</param>
        /// <param name="configSectionPath">配置节路径，默认为"Kestrel"</param>
        /// <returns>Web应用程序构建器</returns>
        public static WebApplicationBuilder UseKestrelServer(this WebApplicationBuilder builder, string configSectionPath = "Kestrel")
        {
            // 获取Kestrel配置节
            var kestrelConfig = builder.Configuration.GetSection(configSectionPath);

            builder.WebHost.ConfigureKestrel(options =>
            {
                try
                {
                    // 配置HTTP端点
                    ConfigureHttpEndpoint(options, kestrelConfig);

                    // 配置HTTPS端点（如果启用）
                    if (kestrelConfig.GetValue<bool>("Https:Enabled", false))
                    {
                        ConfigureHttpsEndpoint(options, kestrelConfig);
                    }
                }
                catch (Exception ex)
                {
                    
                    SiLog.Error("配置Kestrel服务器失败 "+ ex.ToString());

                    // 回退到默认配置
                    ConfigureDefaultEndpoint(options);
                }
            });

            return builder;
        }

        /// <summary>
        /// 配置HTTP端点
        /// </summary>
        private static void ConfigureHttpEndpoint(KestrelServerOptions options, IConfigurationSection config)
        {
            // 获取IP地址，默认为"0.0.0.0"
            var ipString = config.GetValue<string>("Url", "0.0.0.0");
            if (!IPAddress.TryParse(ipString, out var ipAddress))
            {
                ipAddress = IPAddress.Any;
            }

            // 获取HTTP端口，默认为5000
            var port = config.GetValue<int>("Http:Port", 5000);

            // 配置HTTP监听
            options.Listen(ipAddress, port);
        }

        /// <summary>
        /// 配置HTTPS端点
        /// </summary>
        private static void ConfigureHttpsEndpoint(KestrelServerOptions options, IConfigurationSection config)
        {
            // 获取IP地址，默认与HTTP相同
            var ipString = config.GetValue<string>("Url", "0.0.0.0");
            if (!IPAddress.TryParse(ipString, out var ipAddress))
            {
                ipAddress = IPAddress.Any;
            }

            // 获取HTTPS端口，默认为5001
            var port = config.GetValue<int>("Https:Port", 5001);

            // 获取证书配置
            var certPath = config.GetValue<string>("Https:Certificate:Path");
            var certPassword = config.GetValue<string>("Https:Certificate:Password");

            if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
            {
                options.Listen(ipAddress, port, listenOptions =>
                {
                    listenOptions.UseHttps(certPath, certPassword);
                });
            }
        }

        /// <summary>
        /// 配置默认端点（故障回退）
        /// </summary>
        private static void ConfigureDefaultEndpoint(KestrelServerOptions options)
        {
            // 默认监听所有IP地址，端口5000
            options.Listen(IPAddress.Any, 5000);
        }
    }
}
