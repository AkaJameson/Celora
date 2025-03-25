namespace Si.EntityFramework.Extension.Core.Abstractions
{
    /// <summary>
    /// 修改审计接口
    /// </summary>
    public interface IModificationAudited
    {
        string? LastModifiedBy { get; set; }
        DateTime? LastModifiedTime { get; set; }
    }
}