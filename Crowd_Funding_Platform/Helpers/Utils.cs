using System.Security.Cryptography;
using System.Text;

namespace Crowd_Funding_Platform.Helpers
{
    public static class Utils
    {
        public static string CalculateHMAC_SHA256(string data, string secret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
