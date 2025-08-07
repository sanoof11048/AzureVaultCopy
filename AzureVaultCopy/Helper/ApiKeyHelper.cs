using System.Security.Cryptography;
using System.Text;

namespace AzureVaultCopy.Helper
{
    public static class ApiKeyHelper
    {
        public static string GenerateKey(int size = 32)
        {
            var bytes = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static string HashKey(string rawKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
