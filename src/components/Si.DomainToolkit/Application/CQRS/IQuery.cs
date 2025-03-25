using System.Threading;
using System.Threading.Tasks;

namespace Si.DomainToolkit.Application.CQRS
{
    /// <summary>
    /// 查询接口
    /// </summary>
    public interface IQuery<out TResult>
    {
    }

    /// <summary>
    /// 查询处理器接口
    /// </summary>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
    }
} 