using System.Security.Cryptography;
using System.Text;

namespace _Mafia_API.Helpers
{
    public class CryptoHelper
    {
        public static string GenerateMD5(string input)
        {
            using (var md5 = MD5.Create())  
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes);
            }
        }
    }
}