namespace Si.EntityFramework.IdentityServer.Services
{
    public interface IStringHasher
    {
        string GenerateSecurityStamp();
        string HashString(string msg);
        bool IsPasswordInHistory(string password, string passwordHistory);
        string UpdatePasswordHistory(string currentPasswordHash, string passwordHistory, int maxHistoryCount);
        bool VerifyString(string hashString, string @string);
    }
}