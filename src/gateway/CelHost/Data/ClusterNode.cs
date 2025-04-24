using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data
{
    [Table("ClusterNode")]
    public class ClusterNode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

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
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }
    }
}
