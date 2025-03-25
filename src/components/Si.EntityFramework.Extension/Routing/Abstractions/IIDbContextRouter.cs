using Si.EntityFramework.Extension.Data.Context;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;

namespace Si.EntityFramework.Extension.Routing.Abstractions
{
    /// <summary>
    /// 数据库上下文路由接口
    /// </summary>
    /// <typeparam name="TContext">数据库上下文类型</typeparam>
    public interface IDbContextRouter<TContext> where TContext : ApplicationDbContext
    {
        /// <summary>
        /// 获取数据库上下文实例
        /// </summary>
        /// <returns>数据库上下文实例</returns>
        TContext GetContext();
     
        /// <summary>
        /// 强制使用从数据库
        /// </summary>
        /// <returns></returns>
        void ForceSlave();

        /// <summary>
        /// 获取工作单元
        /// </summary>
        /// <returns></returns>
        IUnitOfWork<TContext> GetUnitOfWork();
    }
}
