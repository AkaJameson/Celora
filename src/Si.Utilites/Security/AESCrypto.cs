using System.Security.Cryptography;
using System.Text;

namespace Si.Utilites.Security
{
    static class AESCrypto
    {
        // AES配置常量
        private const int KeySize = 256;
        private const int BlockSize = 128;
        private const CipherMode Mode = CipherMode.CBC;
        private const PaddingMode Padding = PaddingMode.PKCS7;

        /// <summary>
        /// 生成新的密钥和IV对（Base64格式）
        /// </summary>
        public static (string Key, string IV) GenerateKeyAndIV()
        {
            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.GenerateKey();
            aes.GenerateIV();
            return (
                Convert.ToBase64String(aes.Key),
                Convert.ToBase64String(aes.IV)
            );
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="base64Key">Base64格式密钥</param>
        /// <param name="base64IV">Base64格式IV</param>
        public static string Encrypt(string plainText, string base64Key, string base64IV)
        {
            ValidateParameters(plainText, base64Key, base64IV);

            using var aes = CreateAes(base64Key, base64IV);
            using var encryptor = aes.CreateEncryptor();

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="cipherText">Base64格式密文</param>
        /// <param name="base64Key">Base64格式密钥</param>
        /// <param name="base64IV">Base64格式IV</param>
        public static string Decrypt(string cipherText, string base64Key, string base64IV)
        {
            ValidateParameters(cipherText, base64Key, base64IV);

            using var aes = CreateAes(base64Key, base64IV);
            using var decryptor = aes.CreateDecryptor();

            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        private static Aes CreateAes(string base64Key, string base64IV)
        {
            var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = Mode;
            aes.Padding = Padding;

            try
            {
                aes.Key = Convert.FromBase64String(base64Key);
                aes.IV = Convert.FromBase64String(base64IV);

                ValidateKeySize(aes.Key);
                ValidateIVSize(aes.IV);

                return aes;
            }
            catch (FormatException)
            {
                throw new ArgumentException("无效的密钥或IV格式，必须是有效的Base64字符串");
            }
        }

        private static void ValidateParameters(string text, string key, string iv)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(iv)) throw new ArgumentNullException(nameof(iv));
        }

        private static void ValidateKeySize(byte[] key)
        {
            int[] validSizes = { 128, 192, 256 };
            int actualSize = key.Length * 8;

            if (Array.IndexOf(validSizes, actualSize) == -1)
            {
                throw new ArgumentException($"无效的密钥长度：{actualSize}位，必须为128/192/256位");
            }
        }

        private static void ValidateIVSize(byte[] iv)
        {
            if (iv.Length != BlockSize / 8)
            {
                throw new ArgumentException($"IV长度必须为{BlockSize}位（{BlockSize / 8}字节）");
            }
        }

    }
}
