namespace Si.EntityFramework.Extension.Core.Abstractions
{
    /// <summary>
    /// 创建接口
    /// </summary>
    public interface ICreationAudited
    {
        string? CreatedBy { get; set; }
        DateTime? CreatedTime { get; set; }
    }
}