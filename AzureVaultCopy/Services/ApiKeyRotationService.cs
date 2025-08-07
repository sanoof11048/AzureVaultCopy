using AzureVaultCopy.Models;
using AzureVaultCopy.Services;
using System.Reflection;

public class ApiKeyRotationService : BackgroundService
{
    private readonly ILogger<ApiKeyRotationService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ApiKeyRotationService(
        ILogger<ApiKeyRotationService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 API Key Rotation Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var apiKeyService = scope.ServiceProvider.GetRequiredService<IApiKeyService>();
                await RotateKeysIfExpiredAsync(apiKeyService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during key rotation.");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("🛑 API Key Rotation Service stopped.");
    }

    private async Task RotateKeysIfExpiredAsync(IApiKeyService apiKeyService)
    {
        var now = DateTime.UtcNow;
        var keys = (await apiKeyService.GetAllKeyMetadataAsync()).ToList();

        if (!keys.Any())
        {
            _logger.LogWarning("⚠️ No keys found. Creating default key.");
            var raw = await apiKeyService.CreateDefaultKeyAsync();
            _logger.LogInformation($"✅ Default key created. Raw key (store securely)");
            return;
        }

        var expiredKeys = keys
            .Where(k => k.LastRotated.AddMinutes(k.RotationMinutes) <= now)
            .ToList();

        foreach (var key in expiredKeys)
        {
            await apiKeyService.RotateKeyAsync(new ApiKey
            {
                ConfigId = key.ConfigId,
                KeyName = key.KeyName
            }, now);

            _logger.LogInformation($"🔁 Key '{key.KeyName}' rotated. New key (store securely)");
        }

        if (!expiredKeys.Any())
            _logger.LogDebug("✔️ No keys due for rotation.");
    }
}
