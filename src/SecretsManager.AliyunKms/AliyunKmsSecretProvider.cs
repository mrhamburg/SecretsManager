using System.Net.Http.Json;
using SecretsManager.Internal;
using SecretsManager.AliyunKms.Internal;

namespace SecretsManager.AliyunKms;

public sealed class AliyunKmsSecretProvider : ISecretProvider
{
    private readonly IAliyunKmsApiClient _client;

    public AliyunKmsSecretProvider(AliyunKmsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Region))
            throw new ArgumentException("Region is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.AccessKeyId))
            throw new ArgumentException("AccessKeyId is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.AccessKeySecret))
            throw new ArgumentException("AccessKeySecret is required.", nameof(options));

        _client = new AliyunKmsApiClient(options);
    }

    internal AliyunKmsSecretProvider(IAliyunKmsApiClient client)
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
                versionStage: null,
                versionId: query?.Version,
                cancellationToken);

            if (result is null)
                throw new SecretNotFoundException(key);

            var value = result.SecretData;

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
                $"Aliyun KMS error for secret '{key}': {ex.Message}", ex);
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
                key, versionStage: null, versionId: null, cancellationToken);

            AliyunSecretResult result;

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
                $"Aliyun KMS error putting secret '{key}': {ex.Message}", ex);
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
                $"Aliyun KMS error deleting secret '{key}': {ex.Message}", ex);
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
                    IsCurrent = v.VersionStages?.Contains("ACSCurrent") == true
                })
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Aliyun KMS error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.GetSecretValueAsync(
                key, versionStage: null, versionId: null, cancellationToken);
            return result is not null;
        }
        catch (Exception ex)
        {
            throw new SecretProviderException(
                $"Aliyun KMS error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}
