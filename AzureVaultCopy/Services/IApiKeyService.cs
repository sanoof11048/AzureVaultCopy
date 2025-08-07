using AzureVaultCopy.DTOs;
using AzureVaultCopy.Models;

namespace AzureVaultCopy.Services
{
    public interface IApiKeyService
    {
        Task<ApiKeyMetadataDTO?> GetMetadataAsync(string keyName);
        Task<bool> ValidateKeyAsync(string rawKey);
        Task<IEnumerable<ApiKeyMetadataDTO>> GetAllKeyMetadataAsync();
        Task<string?> CreateDefaultKeyAsync();
        Task RotateKeyAsync(ApiKey key, DateTime now);
        Task<string?> GetKeyValueByNameAsync(string keyName);
    }
}
