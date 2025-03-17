using System;

namespace Si.DomainToolkit.Domain.Events
{
    /// <summary>
    /// 领域事件基类
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// 事件发生时间
        /// </summary>
        public DateTime OccurredOn { get; }

        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
    }
} 