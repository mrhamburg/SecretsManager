using Amazon.SecretsManager.Model;
using SecretsManager.AwsSecretsManager.Internal;
using SecretsManager.Internal;

namespace SecretsManager.AwsSecretsManager;

public sealed class AwsSecretsManagerSecretProvider : ISecretProvider
{
    private readonly IAwsSecretsManagerClient _client;

    public AwsSecretsManagerSecretProvider(AwsSecretsManagerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Region))
            throw new ArgumentException("Region is required.", nameof(options));

        _client = new AwsSecretsManagerClient(options);
    }

    internal AwsSecretsManagerSecretProvider(IAwsSecretsManagerClient client)
    {
        _client = client;
    }

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.GetSecretValueAsync(
                key, query?.Version, null, cancellationToken);

            var value = result.Value;

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            var tags = result.Tags?.ToDictionary(
                kvp => kvp.Key, kvp => kvp.Value);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.VersionId,
                CreatedAt = result.CreatedDate,
                Tags = tags?.AsReadOnly(),
            };
        }
        catch (ResourceNotFoundException ex)
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"AWS Secrets Manager error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.PutSecretValueAsync(
                key, value, metadata?.ContentType, cancellationToken);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.VersionId,
                CreatedAt = result.CreatedDate,
                ContentType = metadata?.ContentType,
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"AWS Secrets Manager error putting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteSecretAsync(key, cancellationToken);
        }
        catch (ResourceNotFoundException ex)
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"AWS Secrets Manager error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var versions = new List<SecretVersionInfo>();
            await foreach (var v in _client.ListSecretVersionsAsync(key, cancellationToken))
            {
                versions.Add(new SecretVersionInfo
                {
                    Version = v.VersionId,
                    CreatedAt = v.CreatedDate,
                    IsCurrent = v.IsCurrent,
                });
            }

            return versions.AsReadOnly();
        }
        catch (SecretNotFoundException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"AWS Secrets Manager error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.SecretExistsAsync(key, cancellationToken);
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"AWS Secrets Manager error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}
