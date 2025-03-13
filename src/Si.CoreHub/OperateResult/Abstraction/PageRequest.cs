namespace Si.CoreHub.OperateResult.Abstraction
{
    public interface IPage
    {
        public int? PageIndex { get; set; } 
        public int? PageSize { get; set; }
    }
}
