using Si.Linq2Dapper.Core.Abstraction;

namespace Si.Linq2Dapper.Providers
{
    public class SqlServerProcessor : BaseDbProcessor
    {
        public override string BuildLimitClause(int skip, int take)
            => $"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
    }
    public class MySqlProcessor : BaseDbProcessor
    {
        public override string BuildLimitClause(int skip, int take)
            => $"LIMIT {take} OFFSET {skip}";

        public override string EscapeIdentifier(string name) => $"`{name}`";
    }
    public class PostgreSqlProcessor : BaseDbProcessor
    {
        public override string BuildLimitClause(int skip, int take)
            => $"LIMIT {take} OFFSET {skip}";

        public override string EscapeIdentifier(string name) => $"\"{name}\"";
    }

    public class SqliteProcessor : BaseDbProcessor
    {
        public override string BuildLimitClause(int skip, int take)
            => $"LIMIT {take} OFFSET {skip}";
    }
}
