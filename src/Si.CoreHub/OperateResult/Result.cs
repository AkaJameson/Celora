namespace Si.CoreHub.OperateResult
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public int? TotalCount { get; set; }

        public static Result Successed<T>(T data, string message = "请求成功")
        {
            return new Result()
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static Result Successed(string message = "请求成功")
        {
            return new Result
            {
                Success = true,
                Message = message,
                Data = null
            };
        }
        public static Result Failed(string message = "请求失败")
        {
            return new Result()
            {
                Success = false,
                Message = message,
                Data = null
            };
        }

        public static Result SuccessedByPage<T>(T data, int pageIndex, int pageSize, int totalCount, string message = "请求成功")
        {
            return new Result()
            {
                Success = true,
                Message = message,
                Data = data,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
