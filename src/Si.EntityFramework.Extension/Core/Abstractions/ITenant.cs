namespace Si.EntityFramework.Extension.Core.Abstractions
{
    /// <summary>
    /// 多租户接口
    /// </summary>
    public interface ITenant
    {
        string TenantId { get; set; }
    }
}