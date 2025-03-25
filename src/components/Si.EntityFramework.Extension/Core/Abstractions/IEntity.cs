using System;

namespace Si.EntityFramework.Extension.Core.Abstractions
{
    /// <summary>
    /// 基础实体接口
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// 实体唯一标识符
        /// </summary>
        object Id { get; }
    }

    /// <summary>
    /// 基础泛型实体接口
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    public interface IEntity<TKey> : IEntity where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 实体唯一标识符
        /// </summary>
        new TKey Id { get; set; }
    }
} 