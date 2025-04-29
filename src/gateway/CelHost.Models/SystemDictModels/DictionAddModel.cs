using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.SystemDictModels
{
    public class DictAddNewItemModel
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        [Required]
        public string typeName { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        [Required]
        public string ItemName { get; set; }
        /// <summary>
        /// 属性值
        /// </summary>
        [Required]
        public string ItemValue { get; set; }
        /// <summary>
        /// 属性描述
        /// </summary>
        public string? itemDesc { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? remark { get; set; }
    }
    public class DictAddItemModel
    {
        /// <summary>
        /// 类型编码
        /// </summary>
        public string typeCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string typeName { get; set; }
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
    }
}
