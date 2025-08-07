using AzureVaultCopy.Data;
using AzureVaultCopy.Models;
using AzureVaultCopy.Services;
using Dapper;

public class ApiKeyRotationService : BackgroundService
{
    private readonly ILogger<ApiKeyRotationService> _logger;
    private readonly IApiKeyService _apiKeyService;
    private readonly DapperContext _context;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ApiKeyRotationService(
        ILogger<ApiKeyRotationService> logger,
        IApiKeyService apiKeyService,
        DapperContext context)
    {
        _logger = logger;
        _apiKeyService = apiKeyService;
        _context = context;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 API Key Rotation Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RotateKeysIfExpiredAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during key rotation.");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("🛑 API Key Rotation Service stopped.");
    }

    private async Task RotateKeysIfExpiredAsync()
    {
        using var conn = _context.CreateConnection();
        var now = DateTime.UtcNow;

        var keys = (await conn.QueryAsync<ApiKey>("SELECT * FROM ApiKeyConfigs")).ToList();

        if (!keys.Any())
        {
            _logger.LogWarning("⚠️ No keys found. Creating default key.");
            var raw = await _apiKeyService.CreateDefaultKeyAsync();
            _logger.LogInformation($"✅ Default key created. Raw key (store securely): {raw}");
            return;
        }

        const string rotationSql = @"SELECT * FROM ApiKeyConfigs 
                                     WHERE DATEADD(MINUTE, RotationMinutes, LastRotated) <= @Now";

        var expiredKeys = (await conn.QueryAsync<ApiKey>(rotationSql, new { Now = now })).ToList();

        foreach (var key in expiredKeys)
        {
            await _apiKeyService.RotateKeyAsync(key, now);
            _logger.LogInformation($"🔁 Key '{key.KeyName}' rotated. New key (store securely)");
        }

        if (!expiredKeys.Any())
            _logger.LogDebug("✔️ No keys due for rotation.");
    }
}
