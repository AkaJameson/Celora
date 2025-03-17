using System;

namespace Si.DomainToolkit.Domain.Common
{
    /// <summary>
    /// 实体基类
    /// </summary>
    /// <typeparam name="TId">实体ID类型</typeparam>
    public abstract class BaseEntity<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// 实体ID
        /// </summary>
        public TId Id { get; protected set; }

        protected BaseEntity() { }

        protected BaseEntity(TId id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var other = (BaseEntity<TId>)obj;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(BaseEntity<TId> left, BaseEntity<TId> right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(BaseEntity<TId> left, BaseEntity<TId> right)
        {
            return !(left == right);
        }
    }
} 