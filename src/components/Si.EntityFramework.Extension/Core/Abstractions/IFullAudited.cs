namespace Si.EntityFramework.Extension.Core.Abstractions
{
    public interface IFullAudited : ICreationAudited, IModificationAudited, ISoftDelete
    {
    }
}