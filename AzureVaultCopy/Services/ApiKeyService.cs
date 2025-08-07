using AzureVaultCopy.Data;
using AzureVaultCopy.DTOs;
using AzureVaultCopy.Helper;
using AzureVaultCopy.Models;
using Dapper;

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
            const string sql = "SELECT * FROM ApiKeyConfigs WHERE KeyName = @KeyName";
            using var conn = _context.CreateConnection();
            var key = await conn.QuerySingleOrDefaultAsync<ApiKey>(sql, new { KeyName = keyName });

            return key == null ? null : new ApiKeyMetadataDTO
            {
                KeyName = key.KeyName,
                LastRotated = key.LastRotated,
                RotationMinutes = key.RotationMinutes,
                RotationCount = key.RotationCount
            };
        }

        public async Task<string?> GetKeyValueByNameAsync(string keyName)
        {
            const string sql = "SELECT KeyValue FROM ApiKeyConfigs WHERE KeyName = @KeyName";
            using var conn = _context.CreateConnection();
            return await conn.ExecuteScalarAsync<string?>(sql, new { KeyName = keyName });
        }

        public async Task<bool> ValidateKeyAsync(string rawKey)
        {
            //var hashedKey = ApiKeyHelper.HashKey(rawKey);
            const string sql = "SELECT COUNT(*) FROM ApiKeyConfigs WHERE KeyValue = @KeyValue";
            using var conn = _context.CreateConnection();
            var count = await conn.QuerySingleAsync<int>(sql, new { KeyValue = rawKey });
            return count > 0;
        }


        public async Task<IEnumerable<ApiKeyMetadataDTO>> GetAllKeyMetadataAsync()
        {
            const string sql = "SELECT * FROM ApiKeyConfigs";
            using var conn = _context.CreateConnection();
            var keys = await conn.QueryAsync<ApiKey>(sql);

            return keys.Select(key => new ApiKeyMetadataDTO
            {
                ConfigId = key.ConfigId,
                KeyName = key.KeyName,
                LastRotated = key.LastRotated,
                RotationMinutes = key.RotationMinutes,
                RotationCount = key.RotationCount
            });

        }

        public async Task<string?> CreateDefaultKeyAsync()
        {
            const string defaultName = "DefaultKey";
            using var conn = _context.CreateConnection();
            var existing = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM ApiKeyConfigs WHERE KeyName = @KeyName",
                new { KeyName = defaultName });

            if (existing > 0)
                return null;

            var rawKey = ApiKeyHelper.GenerateKey();
            var hashedKey = ApiKeyHelper.HashKey(rawKey);

            const string sql = @"INSERT INTO ApiKeyConfigs (KeyName, KeyValue, LastRotated, RotationMinutes)
                                 VALUES (@KeyName, @KeyValue, @LastRotated, @RotationMinutes)";

            await conn.ExecuteAsync(sql, new
            {
                KeyName = defaultName,
                KeyValue = hashedKey,
                LastRotated = DateTime.UtcNow,
                RotationMinutes = 1
            });

            return rawKey;
        }

        public async Task RotateKeyAsync(ApiKey key, DateTime now)
        {
            var rawKey = ApiKeyHelper.GenerateKey();
            var hashedKey = ApiKeyHelper.HashKey(rawKey);

            const string updateSql = @"UPDATE ApiKeyConfigs 
                                       SET KeyValue = @KeyValue, LastRotated = @Now, RotationCount = RotationCount + 1 
                                       WHERE ConfigId = @ConfigId";

            using var conn = _context.CreateConnection();
            await conn.ExecuteAsync(updateSql, new
            {
                KeyValue = hashedKey,
                Now = now,
                ConfigId = key.ConfigId
            });
        }
    }
}
