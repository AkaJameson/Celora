using Microsoft.EntityFrameworkCore;

namespace Si.EntityFramework.AutoMigration
{
    /// <summary>
    /// 数据库提供商类型
    /// </summary>
    public enum DatabaseProviderType
    {
        SqlServer,
        Sqlite,
        MySql,
        PostgreSql,
        Unknown
    }
    
    /// <summary>
    /// 数据库提供商检测器
    /// </summary>
    internal static class DatabaseProviderDetector
    {
        /// <summary>
        /// 检测DbContext使用的数据库提供商类型
        /// </summary>
        public static DatabaseProviderType DetectProvider(DbContext context)
        {
            var providerName = context.Database.ProviderName;
            
            if (string.IsNullOrEmpty(providerName))
            {
                throw new InvalidOperationException("无法检测数据库提供商，DbContext未配置数据库提供商。");
            }
            
            if (providerName.Contains("SqlServer"))
            {
                return DatabaseProviderType.SqlServer;
            }
            else if (providerName.Contains("Sqlite"))
            {
                return DatabaseProviderType.Sqlite;
            }
            else if (providerName.Contains("MySql") || providerName.Contains("Pomelo"))
            {
                return DatabaseProviderType.MySql;
            }
            else if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
            {
                return DatabaseProviderType.PostgreSql;
            }
            
            throw new NotSupportedException($"不支持的数据库提供商: {providerName}");
        }
    }
} 