using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Si.AccessControl.Entitys
{
    public class PersonnelInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 用户Id(外键)
        /// </summary>
        public int userId { get; set; }
        /// <summary>
        /// 导航属性
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [Column("basepersonnelname")]
        public string? Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [Column("basepersonnelsex")]
        public string? Sex { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [Column("basepersonnelavatar")]
        public string? Avatar { get; set; }
        /// <summary>
        /// 身份证号码
        /// </summary>
        [Column("basepersonnelidcard")]
        public string? IdCard { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        [Column("basepersonnelbirthday")]
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [Column("basepersonneladdress")]
        public string? Address { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        [Column("basepersonnelnation")]
        public string? Ethnic { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        [Column("basepersonnelphone")]
        public string? Phone { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [Column("basepersonnelemail")]
        public string? Email { get; set; }
        /// <summary>
        /// 微信
        /// </summary>
        [Column("basepersonnelwechat")]
        public string? WeChat { get; set; }
        /// <summary>
        /// QQ
        /// </summary>
        [Column("basepersonnelqq")]
        public string? QQ { get; set; }  
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 保留字段
        /// </summary>
        public string? Reserve1 { get; set; }
        public string? Reserve2 { get; set; }
        public string? Reserve3 { get; set; }
        public string? Reserve4 { get; set; }
        public string? Reserve5 { get; set; }
    }
 
}
