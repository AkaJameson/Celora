namespace Si.EntityFramework.Extension.Core.Abstractions
{
    /// <summary>
    /// �޸���ƽӿ�
    /// </summary>
    public interface IModificationAudited
    {
        string? LastModifiedBy { get; set; }
        DateTime? LastModifiedTime { get; set; }
    }
}