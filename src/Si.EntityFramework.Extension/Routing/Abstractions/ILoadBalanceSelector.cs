using Si.EntityFramework.Extension.Routing.Configuration;

namespace Si.EntityFramework.Extension.Routing.Abstractions
{
    public interface ILoadBalanceSelector
    {
        /// <summary>
        /// 选择一个从库连接
        /// </summary>
        SlaveConnectionConfig Select();
    }
}
