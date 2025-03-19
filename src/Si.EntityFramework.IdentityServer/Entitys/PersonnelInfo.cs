using System.ComponentModel.DataAnnotations.Schema;

namespace Si.EntityFramework.IdentityServer.Entitys
{
    public class PersonnelInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
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
        [Column("BasePersonnelName")]
        public string? Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [Column("BasePersonnelSex")]
        public string? Sex { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [Column("BasePersonnelAvatar")]
        public string? Avatar { get; set; }
        /// <summary>
        /// 身份证号码
        /// </summary>
        [Column("BasePersonnelIdCard")]
        public string? IdCard { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        [Column("BasePersonnelBirthday")]
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [Column("BasePersonnelAddress")]
        public string? Address { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        [Column("BasePersonnelNation")]
        public string? Ethnic { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        [Column("BasePersonnelPhone")]
        public string? Phone { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [Column("BasePersonnelEmail")]
        public string? Email { get; set; }
        /// <summary>
        /// 微信
        /// </summary>
        [Column("BasePersonnelWeChat")]
        public string? WeChat { get; set; }
        /// <summary>
        /// QQ
        /// </summary>
        [Column("BasePersonnelQQ")]
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
