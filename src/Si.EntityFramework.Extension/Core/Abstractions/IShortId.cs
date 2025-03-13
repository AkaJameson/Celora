namespace Si.EntityFramework.Extension.Core.Abstractions
{
    public interface ISnowFlakeId { }
    public interface IShortId : ISnowFlakeId
    {
        public int Id { get; set; }
    }
    public interface ILongId : ISnowFlakeId
    {
        public long Id { get; set; }
    }
}
