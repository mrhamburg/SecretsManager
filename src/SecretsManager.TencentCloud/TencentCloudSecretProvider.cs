using System.Net.Http.Json;
using SecretsManager.Internal;
using SecretsManager.TencentCloud.Internal;

namespace SecretsManager.TencentCloud;

public sealed class TencentCloudSecretProvider : ISecretProvider
{
    private readonly ITencentCloudApiClient _client;

    public TencentCloudSecretProvider(TencentCloudOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Region))
            throw new ArgumentException("Region is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.SecretId))
            throw new ArgumentException("SecretId is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.SecretKey))
            throw new ArgumentException("SecretKey is required.", nameof(options));

        _client = new TencentCloudApiClient(options);
    }

    internal TencentCloudSecretProvider(ITencentCloudApiClient client)
    {
        _client = client;
    }

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.GetSecretValueAsync(
                key,
                versionId: query?.Version,
                cancellationToken);

            if (result is null)
                throw new SecretNotFoundException(key);

            var value = result.SecretValue;

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.VersionId,
                CreatedAt = result.CreateTime,
                UpdatedAt = result.CreateTime
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Tencent Cloud error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var versionId = $"v-{Guid.NewGuid():N}";

            var existing = await _client.GetSecretValueAsync(
                key, versionId: null, cancellationToken);

            TencentCloudSecretResult result;

            if (existing is null)
            {
                result = await _client.CreateSecretAsync(key, value, versionId, cancellationToken);
            }
            else
            {
                result = await _client.PutSecretValueAsync(key, value, versionId, cancellationToken);
            }

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.VersionId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Tencent Cloud error putting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task DeleteSecretAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteSecretAsync(key, cancellationToken);
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Tencent Cloud error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var versions = await _client.ListSecretVersionIdsAsync(key, cancellationToken);

            return versions
                .OrderByDescending(v => v.CreateTime)
                .Select(v => new SecretVersionInfo
                {
                    Version = v.VersionId,
                    CreatedAt = v.CreateTime,
                    IsCurrent = v.VersionStages?.Contains("Latest") == true
                })
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Tencent Cloud error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.GetSecretValueAsync(
                key, versionId: null, cancellationToken);
            return result is not null;
        }
        catch (Exception ex)
        {
            throw new SecretProviderException(
                $"Tencent Cloud error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}
