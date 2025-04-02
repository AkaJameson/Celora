using Microsoft.EntityFrameworkCore;

namespace Si.EntityFramework.Extension.Routing.Abstractions
{
    /// <summary>
    /// 数据库上下文路由接口
    /// </summary>
    /// <typeparam name="TContext">数据库上下文类型</typeparam>
    public interface IDbContextRouter<TContext> where TContext : DbContext
    {
        /// <summary>
        /// 获取数据库上下文实例
        /// </summary>
        /// <returns>数据库上下文实例</returns>
        TContext GetWriteContext();
        /// <summary>
        /// 获取从数据库上下文实例
        /// </summary>
        /// <returns></returns>
        TContext GetReadContext();

    }
}
