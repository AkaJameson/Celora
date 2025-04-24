using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data
{
    [Table("Cluster")]
    public class Cluster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 集群名称
        /// </summary>
        [Required, MaxLength(100)]
        public string ClusterName { get; set; }
        /// <summary>
        /// 路由唯一标识（对应YARP RouteConfig.RouteId）
        /// </summary>
        [Required, MaxLength(100)]
        public string RouteId { get; set; }
        /// <summary>
        /// 路由前匹配
        /// </summary>
        [Required, MaxLength(500)]
        public string Path { get; set; }
        /// <summary>
        /// 速率限制策略名称
        /// </summary>
        public string RateLimitPolicyName { get; set; }
        /// <summary>
        /// 负载均衡策略
        /// </summary>
        public string LoadBalancingPolicy { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// 集群配置
        /// </summary>
        public virtual IList<ClusterNode> Nodes { get; set; } = new List<ClusterNode>();
        /// <summary>
        /// 健康检查端点（可选）
        /// </summary>
        public bool HealthCheck { get; set; } = true;
        /// <summary>
        /// 健康检查配置Id
        /// </summary>
        public int? HealthCheckId { get; set; }
        /// <summary>
        /// 健康检查政策
        /// </summary>
        public virtual HealthCheckOption CheckOption { get; set; }
    }
}
