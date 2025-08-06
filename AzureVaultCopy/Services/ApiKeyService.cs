using AzureVaultCopy.Data;
using AzureVaultCopy.DTOs;
using AzureVaultCopy.Models;
using Dapper;
using System.Security.Cryptography;
using System.Text;

namespace AzureVaultCopy.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly DapperContext _context;

        public ApiKeyService(DapperContext context)
        {
            _context = context;
        }

        public async Task<ApiKeyMetadataDTO?> GetMetadataAsync(string keyName)
        {
            var sql = "SELECT * FROM ApiKeyConfigs WHERE KeyName = @KeyName";
            using var conn = _context.CreateConnection();
            var key = await conn.QuerySingleOrDefaultAsync<ApiKey>(sql, new { KeyName = keyName });

            return key == null ? null : new ApiKeyMetadataDTO
            {
                KeyName = key.KeyName,
                LastRotated = key.LastRotated,
                RotationHours = key.RotationHours
            };
        }

        public async Task<bool> ValidateKeyAsync(string providedKey)
        {
            var hashed = HashKey(providedKey);
            var sql = "SELECT COUNT(1) FROM ApiKeyConfigs WHERE KeyValue = @KeyValue";
            using var conn = _context.CreateConnection();
            var exists = await conn.ExecuteScalarAsync<int>(sql, new { KeyValue = hashed });
            return exists > 0;
        }

        private static string HashKey(string rawKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        public async Task<IEnumerable<ApiKey>> GetAllKeysAsync()
        {
            const string sql = "SELECT * FROM ApiKeyConfigs";
            using var conn = _context.CreateConnection();
            return await conn.QueryAsync<ApiKey>(sql);
        }

    }
}
