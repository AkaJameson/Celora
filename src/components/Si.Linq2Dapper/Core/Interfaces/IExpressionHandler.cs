using Dapper;
using System.Linq.Expressions;

namespace Si.Linq2Dapper.Core.Interfaces
{
    internal interface IExpressionHandler
    {
        bool CanHandle(Expression expression);
        // 处理表达式并生成SQL片段
        string Handle(
            Expression expression,
            IDbProcessor processor,
            out DynamicParameters parameters
        );
    }
}
