using CelHost.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelHost.Data.Data
{
    /// <summary>
    /// 下级网关级联信息
    /// </summary>
    [Table("Cascade")]
    public class Cascade
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 下级网关名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 下级网关地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 下级网关路由    
        /// </summary>
        public string Route { get; set; }
        /// <summary>
        /// 下级网关账号
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 下级网关密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }
    }
}
