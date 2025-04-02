using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;
using Si.EntityFramework.Extension.Routing.Interceptor;

namespace Si.EntityFramework.Extension.Routing.Implementations
{
    /// <summary>
    /// 数据库上下文路由实现类
    /// </summary>
    /// <typeparam name="TContext">数据库上下文类型</typeparam>
    public class DbContextRouter<TContext> : IDbContextRouter<TContext> where TContext : DbContext
    {
        private readonly RoutingOptions _options;
    
        private TContext writeContext;
        private TContext readContext;
        private readonly IServiceProvider serviceProvider;
        //private readonly DbModel
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        /// <param name="options">路由配置选项</param>
        public DbContextRouter(IServiceProvider serviceProvider, TContext context)
        {
            this.writeContext = context;
            this.serviceProvider = serviceProvider;
        }
        /// <summary>
        /// 获取写入数据库上下文
        /// </summary>
        /// <returns></returns>
        public TContext GetWriteContext() => writeContext;
        /// <summary>
        /// 获取读数据库上下文
        /// </summary>
        /// <returns></returns>
        public TContext GetReadContext()
        {
            if(readContext == null)
            {
                var originalOptionsBuilder = serviceProvider.GetRequiredService<DbContextOptionsBuilder<TContext>>();
                var newOptionsBuilder = new DbContextOptionsBuilder<TContext>(originalOptionsBuilder.Options);
                newOptionsBuilder.AddInterceptors(new ReadForceInterceptor<TContext>());
                newOptionsBuilder.AddInterceptors(new ConnectionInterceptor<TContext>(serviceProvider));
                readContext = ActivatorUtilities.CreateInstance<TContext>(serviceProvider,newOptionsBuilder.Options);
            }
            return readContext;
        }
    }
}
