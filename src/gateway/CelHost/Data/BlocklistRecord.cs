using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data
{
    /// <summary>
    /// 黑名单记录
    /// </summary>
    [Table("BlocklistRecord")]
    public class BlocklistRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 封禁的ip
        /// </summary>
        public string BlockIp { get; set; }
        /// <summary>
        /// 封禁原因
        /// </summary>
        public string BlockReason { get; set; }
        /// <summary>
        /// 封禁次数
        /// </summary>
        public int BlockCount { get; set; }
        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime EffectiveTime { get; set; }
        /// <summary>
        /// 解封时间
        /// </summary>
        public DateTime ExpireTime { get; set; } 
    }
}
