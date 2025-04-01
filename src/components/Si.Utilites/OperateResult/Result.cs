namespace Si.Utilites.OperateResult
{
    public class OperateResult<T> : OperateResult
    {
        public T Data { get; set; }
    }
    public class OperateResult
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }
        public static OperateResult Failed(string message = "操作失败")
        {
            string value;
            return new OperateResult
            {
                Succeeded = false,
                Code = -1,
                Message = message
            };
        }
        public static OperateResult<T> Successed<T>(T data)
        {
            return new OperateResult<T>
            {
                Code = 0,
                Message = "操作成功",
                Succeeded = true,
                Data = data
            };
        }
        public static OperateResult Successed()
        {
            return new OperateResult
            {
                Code = 200,
                Message = "操作成功",
                Succeeded = true
            };
        }
    }
}
