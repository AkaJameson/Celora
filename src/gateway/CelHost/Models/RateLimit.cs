namespace CelHost.Models
{
    public class RateLimit
    {
        /// <summary>
        /// 窗口大小，单位秒
        /// </summary>
        public int WindowSize { get; set; }
        /// <summary>
        /// 最大请求数
        /// </summary>
        public int MaxRequests { get; set; }
        /// <summary>
        /// 队列长度
        /// </summary>
        public int QueueLimit { get; set; }
    }
}
