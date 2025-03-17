using System;
using System.Linq.Expressions;

namespace Si.DomainToolkit.Domain.Specifications
{
    /// <summary>
    /// 规范接口
    /// </summary>
    /// <typeparam name="T">规范应用的实体类型</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// 检查实体是否满足规范
        /// </summary>
        /// <param name="entity">待检查的实体</param>
        /// <returns>是否满足规范</returns>
        bool IsSatisfiedBy(T entity);

        /// <summary>
        /// 获取规范的表达式
        /// </summary>
        /// <returns>规范表达式</returns>
        Expression<Func<T, bool>> ToExpression();
    }
} 