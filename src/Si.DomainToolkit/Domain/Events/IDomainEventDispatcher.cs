using System.Threading;
using System.Threading.Tasks;

namespace Si.DomainToolkit.Domain.Events
{
    /// <summary>
    /// 领域事件发布器接口
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// 发布领域事件
        /// </summary>
        /// <param name="domainEvent">领域事件</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }
} 