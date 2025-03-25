using Si.EntityFramework.IdentityServer.ServicesImpl;

namespace Si.EntityFramework.IdentityServer.Models
{
    public record RefreshTokenModel
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 截至时间
        /// </summary>
        public DateTime Deadline { get; set; }
        /// <summary>
        /// 安全戳
        /// </summary>
        public string securtyStamp { get; set; } = new StringHasher().GenerateSecurityStamp();
    }
}
