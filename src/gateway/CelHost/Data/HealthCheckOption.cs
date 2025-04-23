using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data
{
    public class HealthCheckOption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 限流名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 检查间隔
        /// </summary>
        [Required]
        public int Interval { get; set; }
        /// <summary>
        /// 超时时间
        /// </summary>
        [Required]
        public int Timeout { get; set; }
        /// <summary>
        /// 检查路径
        /// </summary>
        [Required, StringLength(255)]
        public string ActivePath { get; set; } = "/health";
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// 集群关联
        /// </summary>
        public virtual IList<Cluster> Clusters { get; set; } = new List<Cluster>();
    }
}
