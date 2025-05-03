namespace CelHost.Apis.Models
{
    public class OperateResult
    {
        public int Code { get; set; } = -1;
        public string Message { get; set; } = "未知错误";
        public bool Succeeded { get; set; } = false;
    }

    public class OperateResult<T> : OperateResult
    {
        public T Data { get; set; }
    }
}
