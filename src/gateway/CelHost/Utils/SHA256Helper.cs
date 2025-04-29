using System.Security.Cryptography;
using System.Text;

namespace CelHost.Utils
{
    public class SHA256Helper
    {
        public static string Encrypt(string input)
        {
            byte[] tmpByte;
            using (SHA256 sha256 = SHA256.Create())
            {
                tmpByte = sha256.ComputeHash(GetKeyByteArray(input));
            }
            StringBuilder rst = new StringBuilder();
            for (int i = 0; i < tmpByte.Length; i++)
            {
                rst.Append(tmpByte[i].ToString("x2"));
            }
            return rst.ToString();
        }
        private static byte[] GetKeyByteArray(string strKey)
        {
            UTF8Encoding Asc = new UTF8Encoding();
            int tmpStrLen = strKey.Length;
            byte[] tmpByte = new byte[tmpStrLen - 1];
            tmpByte = Asc.GetBytes(strKey);
            return tmpByte;
        }
    }
}
