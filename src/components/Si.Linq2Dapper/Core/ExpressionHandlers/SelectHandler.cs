using Dapper;
using Si.Linq2Dapper.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Si.Linq2Dapper.Core.ExpressionHandlers
{
    public class SelectHandler : IExpressionHandler
    {
        public bool CanHandle(Expression expression)
        {
            throw new NotImplementedException();
        }

        string IExpressionHandler.Handle(Expression expression, IDbProcessor processor, out DynamicParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
