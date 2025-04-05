using Si.Linq2Dapper.Core.Interfaces;
using System.Data;

namespace Si.Linq2Dapper.Core.Abstraction
{
    public abstract class BaseDbProcessor : IDbProcessor
    {
        public virtual string EscapeIdentifier(string name) => $"[{name}]";

        public abstract string BuildLimitClause(int skip, int take);

        public virtual string ParameterPrefix => "@";
    }
}
