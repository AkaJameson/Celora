namespace Si.EntityFramework.Extension.Core.Abstractions
{
    /// <summary>
    /// �����ӿ�
    /// </summary>
    public interface ICreationAudited
    {
        string? CreatedBy { get; set; }
        DateTime? CreatedTime { get; set; }
    }
}