using CelHost.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data.Data
{
    [Table("ClusterNode")]
    public class ClusterNode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClusterId { get; set; }

        [ForeignKey("ClusterId")]
        public virtual Cluster Cluster { get; set; }

        /// <summary>
        /// 目标地址（格式：http://host:port）
        /// </summary>
        [Required, MaxLength(500)]
        public string Address { get; set; }

        /// <summary>
        /// 健康检查端点（可选）
        /// </summary>
        [MaxLength(500)]
        public string HealthCheck { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 最后健康检查时间
        /// </summary>
        public DateTime? LastHealthCheck { get; set; }
    }
}
