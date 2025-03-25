using System;

namespace Si.DomainToolkit.Domain.Events
{
    /// <summary>
    /// 领域事件接口
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// 事件发生时间
        /// </summary>
        DateTime OccurredOn { get; }
    }
} 