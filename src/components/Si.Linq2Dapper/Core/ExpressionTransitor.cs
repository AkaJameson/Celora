using Dapper;
using System.Data;
using System.Linq.Expressions;

namespace Si.Linq2Dapper.Core
{
    internal class ExpressionTransitor:ExpressionVisitor
    {
        private DynamicParameters _parameters = new();
        private IDbTransaction dbTransaction;
        private express
        public ExpressionTransitor()
        {
            
        }
    }
}
