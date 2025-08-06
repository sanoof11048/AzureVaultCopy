using AzureVaultCopy.Data;
using AzureVaultCopy.Models;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

public class ApiKeyRotationService : BackgroundService
{
    private readonly ILogger<ApiKeyRotationService> _logger;
    private readonly DapperContext _context;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ApiKeyRotationService(ILogger<ApiKeyRotationService> logger, DapperContext context)
    {
        _logger = logger;
        _context = context;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("API Key Rotation Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RotateExpiredKeysAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during API key rotation.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("API Key Rotation Service cancellation requested.");
                break;
            }
        }

        _logger.LogInformation("API Key Rotation Service stopped.");
    }

    private async Task RotateExpiredKeysAsync()
    {
        try
        {
            using var conn = _context.CreateConnection();

            var now = DateTime.UtcNow;

            var existingKeys = await conn.QueryAsync<ApiKey>("SELECT * FROM ApiKeyConfigs");

            if (!existingKeys.Any())
            {
                _logger.LogWarning("No API keys found. Creating a default key.");

                var rawKey = GenerateApiKey();
                var hashedKey = HashKey(rawKey);

                const string insert = @"INSERT INTO ApiKeyConfigs (KeyName, KeyValue, LastRotated, RotationMinutes)
                                        VALUES (@KeyName, @KeyValue, @LastRotated, @RotationMinutes)";

                await conn.ExecuteAsync(insert, new
                {
                    KeyName = "DefaultKey",
                    KeyValue = hashedKey,
                    LastRotated = now,
                    RotationMinutes = 1
                });

                _logger.LogInformation("Default API key created and inserted.");
                return; 
            }


            const string query = @"
                SELECT * FROM ApiKeyConfigs 
                WHERE DATEADD(HOUR, RotationMinutes, LastRotated) <= @Now";

            var expiringKeys = (await conn.QueryAsync<ApiKey>(query, new { Now = now })).ToList();

            foreach (var key in expiringKeys)
            {
                var newRawKey = GenerateApiKey();
                var newHashed = HashKey(newRawKey);

                const string update = @"UPDATE ApiKeyConfigs 
                                        SET KeyValue = @KeyValue, LastRotated = @Now 
                                        WHERE ConfigId = @ConfigId";

                await conn.ExecuteAsync(update, new
                {
                    KeyValue = newHashed,
                    Now = now,
                    ConfigId = key.ConfigId
                });

                _logger.LogInformation($"API Key '{key.KeyName}' rotated at {now}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in RotateExpiredKeysAsync.");
            throw;
        }
    }

    private static string GenerateApiKey(int size = 32)
    {
        var bytes = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashKey(string rawKey)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawKey);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
