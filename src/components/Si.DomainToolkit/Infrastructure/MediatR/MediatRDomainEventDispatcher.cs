using MediatR;
using Si.DomainToolkit.Domain.Events;

namespace Si.DomainToolkit.Infrastructure.MediatR
{
    /// <summary>
    /// 基于 MediatR 的领域事件分发器
    /// </summary>
    public class MediatRDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;

        public MediatRDomainEventDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
} 