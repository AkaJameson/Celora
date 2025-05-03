namespace CelHost.Apis.Models
{
    public class OperateResult
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }
    }

    public class OperateResult<T> : OperateResult
    {
        public T Data { get; set; }
    }
}
