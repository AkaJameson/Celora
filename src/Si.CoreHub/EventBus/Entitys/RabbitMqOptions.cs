namespace Si.CoreHub.EventBus.Entitys
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost"; // 默认值
        public int Port { get; set; } = 5672; // 默认端口
        public string UserName { get; set; } = "guest"; // 默认用户名
        public string Password { get; set; } = "guest"; // 默认密码
    }
}
