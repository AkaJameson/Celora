using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Server.Data
{
    // 第一步：修改数据库实体
    [Table("BlocklistRecord")]
    public class BlocklistRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BlockIp { get; set; }

        [Required]
        [MaxLength(200)]
        public string BlockReason { get; set; }

        [Required]
        public int BlockCount { get; set; } = 1;

        [Required]
        public DateTime EffectiveTime { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpireTime { get; set; }

        [Required]
        public bool IsPermanent { get; set; } = false;

        // 新增最后违规时间
        public DateTime LastViolationTime { get; set; } = DateTime.UtcNow;
    }

}
