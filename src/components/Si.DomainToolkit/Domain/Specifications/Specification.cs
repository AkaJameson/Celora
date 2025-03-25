using System;
using System.Linq.Expressions;

namespace Si.DomainToolkit.Domain.Specifications
{
    /// <summary>
    /// 规范基类
    /// </summary>
    /// <typeparam name="T">规范应用的实体类型</typeparam>
    public abstract class Specification<T> : ISpecification<T>
    {
        /// <summary>
        /// 获取规范的表达式
        /// </summary>
        public abstract Expression<Func<T, bool>> ToExpression();

        /// <summary>
        /// 检查实体是否满足规范
        /// </summary>
        public bool IsSatisfiedBy(T entity)
        {
            var predicate = ToExpression().Compile();
            return predicate(entity);
        }

        /// <summary>
        /// 与操作
        /// </summary>
        public Specification<T> And(Specification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        /// <summary>
        /// 或操作
        /// </summary>
        public Specification<T> Or(Specification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }

        /// <summary>
        /// 非操作
        /// </summary>
        public Specification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }

    internal sealed class AndSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public AndSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();
            var paramExpr = Expression.Parameter(typeof(T));
            var andExpression = Expression.AndAlso(
                leftExpression.Body.ReplaceParameter(leftExpression.Parameters[0], paramExpr),
                rightExpression.Body.ReplaceParameter(rightExpression.Parameters[0], paramExpr)
            );
            return Expression.Lambda<Func<T, bool>>(andExpression, paramExpr);
        }
    }

    internal sealed class OrSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public OrSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();
            var paramExpr = Expression.Parameter(typeof(T));
            var orExpression = Expression.OrElse(
                leftExpression.Body.ReplaceParameter(leftExpression.Parameters[0], paramExpr),
                rightExpression.Body.ReplaceParameter(rightExpression.Parameters[0], paramExpr)
            );
            return Expression.Lambda<Func<T, bool>>(orExpression, paramExpr);
        }
    }

    internal sealed class NotSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _specification;

        public NotSpecification(Specification<T> specification)
        {
            _specification = specification;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var expression = _specification.ToExpression();
            var notExpression = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(notExpression, expression.Parameters.Single());
        }
    }

    internal static class ExpressionExtensions
    {
        public static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target)
        {
            return new ParameterReplacer { Source = source, Target = target }.Visit(expression);
        }
    }

    internal class ParameterReplacer : ExpressionVisitor
    {
        public ParameterExpression Source;
        public Expression Target;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == Source ? Target : base.VisitParameter(node);
        }
    }
} 