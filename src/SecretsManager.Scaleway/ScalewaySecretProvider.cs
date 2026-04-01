using SecretsManager.Internal;
using SecretsManager.Scaleway.Internal;

namespace SecretsManager.Scaleway;

public sealed class ScalewaySecretProvider : ISecretProvider
{
    private readonly IScalewayApiClient _client;

    public ScalewaySecretProvider(ScalewayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Region))
            throw new ArgumentException("Region is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.ProjectId))
            throw new ArgumentException("ProjectId is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.SecretKey))
            throw new ArgumentException("SecretKey is required.", nameof(options));

        _client = new ScalewayApiClient(options);
    }

    internal ScalewaySecretProvider(IScalewayApiClient client) =>
        _client = client;

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.FindSecretByNameAsync(key, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            var revision = query?.Version ?? "latest_enabled";
            var access = await _client.AccessVersionAsync(secret.Id, revision, cancellationToken);
            var value = access.Data;

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = access.Revision.ToString(),
                CreatedAt = secret.CreatedAt,
                UpdatedAt = secret.UpdatedAt
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Scaleway error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.FindSecretByNameAsync(key, cancellationToken);

            if (secret is null)
            {
                var tags = metadata?.Tags?.Keys.ToArray();
                secret = await _client.CreateSecretAsync(key, tags, cancellationToken);
            }

            var version = await _client.CreateVersionAsync(secret.Id, value, cancellationToken);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = version.Revision.ToString(),
                CreatedAt = version.CreatedAt,
                UpdatedAt = version.UpdatedAt
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Scaleway error putting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.FindSecretByNameAsync(key, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            await _client.DeleteSecretAsync(secret.Id, cancellationToken);
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Scaleway error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.FindSecretByNameAsync(key, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            var versions = await _client.ListVersionsAsync(secret.Id, cancellationToken);

            return versions
                .Select(v => new SecretVersionInfo
                {
                    Version = v.Revision.ToString(),
                    CreatedAt = v.CreatedAt,
                    IsCurrent = v.Latest
                })
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Scaleway error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.FindSecretByNameAsync(key, cancellationToken);
            return secret is not null;
        }
        catch (Exception ex)
        {
            throw new SecretProviderException(
                $"Scaleway error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}
