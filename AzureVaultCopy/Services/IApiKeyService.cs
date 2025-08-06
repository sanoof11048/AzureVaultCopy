using AzureVaultCopy.DTOs;
using AzureVaultCopy.Models;

namespace AzureVaultCopy.Services
{
    public interface IApiKeyService
    {
        Task<ApiKeyMetadataDTO?> GetMetadataAsync(string keyName);
        Task<bool> ValidateKeyAsync(string providedKey);
        Task<IEnumerable<ApiKey>> GetAllKeysAsync();

    }
}
