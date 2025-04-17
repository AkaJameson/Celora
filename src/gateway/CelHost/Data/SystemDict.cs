using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data
{
    public class SystemDict
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PkId { get; set; }
        /// <summary>
        /// 类型编码
        /// </summary>
        public string typeCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string typeName { get; set; }
        /// <summary>
        /// 属性编码
        /// </summary>
        public string itemCode { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        public string itemName { get; set; }
        /// <summary>
        /// 属性值
        /// </summary>
        public string itemValue { get; set; }
        /// <summary>
        /// 属性描述
        /// </summary>
        public string? itemDesc { get; set; }
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
        /// <summary>
        /// 父类型编码
        /// </summary>
        public string? superTypeCode { get; set; }
    }
}
