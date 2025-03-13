
using Si.EntityFramework.Extension.Core.Abstractions;

namespace Si.EntityFramework.Extension.Core.Entitys
{
    public abstract class AuditedEntityBase : IFullAudited
    {
        public string? CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedTime { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedTime { get; set; }
        public string? DeletedBy { get; set; }
    }
}