namespace CelHost.Utils
{
    using System;
    using Serilog;
    using Serilog.Context;
    using Serilog.Core;
    using Serilog.Events;

    public static class LogCenter
    {
        private static Logger _logger = InitializeDefaultLogger();
        private static LoggerConfiguration _loggerConfiguration;

        public static ILogger Logger { get; set; } = _logger;

        /// <summary>
        /// 使用自定义配置初始化日志记录器
        /// </summary>
        public static void Initialize(LoggerConfiguration configuration = null)
        {
            _loggerConfiguration = configuration ?? CreateDefaultConfiguration();
            try
            {
                CloseAndFlush();
                _logger = _loggerConfiguration.CreateLogger();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize logger", ex);
            }
        }
        /// <summary>
        /// 记录调试信息
        /// </summary>
        public static void Debug(string message, Exception exception = null, params object[] propertyValues)
        {
            Write(LogEventLevel.Debug, message, exception, propertyValues);
        }

        /// <summary>
        /// 记录普通信息
        /// </summary>
        public static void Information(string message, Exception exception = null, params object[] propertyValues)
        {
            Write(LogEventLevel.Information, message, exception, propertyValues);
        }

        /// <summary>
        /// 记录警告信息
        /// </summary>
        public static void Warning(string message, Exception exception = null, params object[] propertyValues)
        {
            Write(LogEventLevel.Warning, message, exception, propertyValues);
        }

        /// <summary>
        /// 记录错误信息
        /// </summary>
        public static void Error(string message, Exception exception = null, params object[] propertyValues)
        {
            Write(LogEventLevel.Error, message, exception, propertyValues);
        }

        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        public static void Fatal(string message, Exception exception = null, params object[] propertyValues)
        {
            Write(LogEventLevel.Fatal, message, exception, propertyValues);
        }

        /// <summary>
        /// 添加上下文属性（自动释放）
        /// </summary>
        public static IDisposable PushProperty(string key, object value)
        {
            return LogContext.PushProperty(key, value);
        }

        /// <summary>
        /// 关闭并刷新日志
        /// </summary>
        public static void CloseAndFlush()
        {
            _logger.Dispose();
        }

        private static void Write(LogEventLevel level, string message, Exception exception, object[] propertyValues)
        {
            if (exception != null)
            {
                Logger.Write(level, exception, message, propertyValues);
            }
            else
            {
                Logger.Write(level, message, propertyValues);
            }
        }

        private static Logger InitializeDefaultLogger()
        {
            return CreateDefaultConfiguration().CreateLogger();
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <returns></returns>
        private static LoggerConfiguration CreateDefaultConfiguration()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.File("../logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}");
        }
    }
}
