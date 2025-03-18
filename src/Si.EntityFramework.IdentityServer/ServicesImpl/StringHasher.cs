using System.Security.Cryptography;
using System.Text;
using Si.EntityFramework.IdentityServer.Services;

namespace Si.EntityFramework.IdentityServer.ServicesImpl
{
    /// <summary>
    /// 密码哈希工具
    /// </summary>
    public class StringHasher : IStringHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;
        private const char Delimiter = ':';

        /// <summary>
        /// 哈希
        /// </summary>
        /// <param name="msg">明文密码</param>
        /// <returns>哈希后的密码字符串（格式：salt:hash:iterations:algorithm）</returns>
        public string HashString(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                throw new ArgumentException("密码不能为空", nameof(msg));

            // 生成随机盐值
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            // 使用PBKDF2算法生成哈希
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(msg),
                salt,
                Iterations,
                _hashAlgorithm,
                KeySize);

            // 将盐值、哈希、迭代次数和算法名称组合为一个字符串
            return string.Join(
                Delimiter.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash),
                Iterations,
                _hashAlgorithm.Name);
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="hashString">哈希后的密码字符串</param>
        /// <param name="string">要验证的明文密码</param>
        /// <returns>密码是否正确</returns>
        public bool VerifyString(string hashString, string @string)
        {
            if (string.IsNullOrEmpty(hashString))
                throw new ArgumentException("哈希密码不能为空", nameof(hashString));

            if (string.IsNullOrEmpty(@string))
                throw new ArgumentException("密码不能为空", nameof(@string));

            var parts = hashString.Split(Delimiter);
            if (parts.Length != 4)
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[0]);
                var hash = Convert.FromBase64String(parts[1]);
                var iterations = int.Parse(parts[2]);
                var algorithm = new HashAlgorithmName(parts[3]);
                var testHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(@string),
                    salt,
                    iterations,
                    algorithm,
                    hash.Length);
                return CryptographicOperations.FixedTimeEquals(hash, testHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 生成随机安全戳
        /// </summary>
        /// <returns>安全戳字符串</returns>
        public string GenerateSecurityStamp()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 检查密码是否在历史记录中
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="passwordHistory">密码历史记录，使用分号分隔</param>
        /// <returns>是否在历史记录中</returns>
        public bool IsPasswordInHistory(string password, string passwordHistory)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHistory))
                return false;

            var historyItems = passwordHistory.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in historyItems)
            {
                if (VerifyString(item, password))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 更新密码历史记录
        /// </summary>
        /// <param name="currentPasswordHash">当前密码哈希</param>
        /// <param name="passwordHistory">现有密码历史记录</param>
        /// <param name="maxHistoryCount">历史记录最大数量</param>
        /// <returns>更新后的密码历史记录</returns>
        public string UpdatePasswordHistory(string currentPasswordHash, string passwordHistory, int maxHistoryCount)
        {
            if (string.IsNullOrEmpty(currentPasswordHash))
                throw new ArgumentException("当前密码哈希不能为空", nameof(currentPasswordHash));

            if (maxHistoryCount <= 0)
                throw new ArgumentException("历史记录最大数量必须大于0", nameof(maxHistoryCount));

            var historyItems = string.IsNullOrEmpty(passwordHistory)
                ? new List<string>()
                : passwordHistory.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

            // 添加当前密码到历史记录
            historyItems.Add(currentPasswordHash);

            // 如果超出最大数量，移除最旧的记录
            while (historyItems.Count > maxHistoryCount)
            {
                historyItems.RemoveAt(0);
            }

            return string.Join(';', historyItems);
        }
    }
}