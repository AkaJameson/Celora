using CelHost.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data
{
    [Table("ClusterNode")]
    public class ClusterNode
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 集群ID
        /// </summary>
        public int ClusterId { get; set; }
        /// <summary>
        /// 集群
        /// </summary>
        public virtual Cluster Cluster { get; set; }
        /// <summary>
        /// 节点URL
        /// </summary>
        public string HostUrl { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 节点状态
        /// </summary>
        public NodeStatus Status { get; set; }
        /// <summary>
        /// 节点描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }
    }
}
