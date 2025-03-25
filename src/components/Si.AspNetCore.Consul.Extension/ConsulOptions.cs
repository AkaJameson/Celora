namespace Si.AspNetCore.Consul.Extension;

public class ConsulOptions
{
    /// <summary>
    /// Consul 服务器地址
    /// </summary>
    public string Address { get; set; } = "http://localhost:8500";

    /// <summary>
    /// 服务名称
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// 服务ID
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// 服务地址
    /// </summary>
    public string ServiceAddress { get; set; } = string.Empty;

    /// <summary>
    /// 服务端口
    /// </summary>
    public int ServicePort { get; set; }

    /// <summary>
    /// 健康检查地址
    /// </summary>
    public string HealthCheckUrl { get; set; } = "/health";

    /// <summary>
    /// 健康检查间隔时间（秒）
    /// </summary>
    public int HealthCheckInterval { get; set; } = 10;

    /// <summary>
    /// 健康检查超时时间（秒）
    /// </summary>
    public int HealthCheckTimeout { get; set; } = 5;

    /// <summary>
    /// 服务标签
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();
} 