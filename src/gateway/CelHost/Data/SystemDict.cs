using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Server.Data
{
    public class SystemDict
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? ParentId { get; set; }
        /// <summary>
        /// 类型编码
        /// </summary>
        public string typeCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string typeName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int order { get; set; }
    }
}
