using CelHost.Data.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;

namespace CelHost.Data
{
    /// <summary>
    /// 请求限流功能
    /// </summary>
    public class RateLimitPolicy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 限流策略名称
        /// </summary>
        public string PolicyName { get; set; }
        /// <summary>
        /// 允许请求数量
        /// </summary>
        public int PermitLimit { get; set; }
        /// <summary>
        /// 时间窗口
        /// </summary>
        public TimeSpan Window { get; set; }
        /// <summary>
        /// 队列处理顺序
        /// </summary>
        public QueueProcessingOrder QueueProcessingOrder { get; set; }
        /// <summary>
        /// 队列大小限制
        /// </summary>
        public int QueueLimit { get; set; }
        public virtual IList<Cluster> Clusters { get; set; } = new List<Cluster>();
    }
}
