namespace Si.Linq2Dapper.Core.Interfaces
{
    internal interface IDbProcessor
    {
        string BuildLimitClause(int skip, int take);  
        string ParameterPrefix { get; }               
        string EscapeIdentifier(string name);         
    }
}
