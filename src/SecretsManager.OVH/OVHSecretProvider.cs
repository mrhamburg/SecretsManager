using SecretsManager.Internal;
using SecretsManager.OVH.Internal;

namespace SecretsManager.OVH;

public sealed class OVHSecretProvider : ISecretProvider
{
    private readonly IOVHApiClient _client;

    public OVHSecretProvider(OVHOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("Endpoint is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.ApplicationKey))
            throw new ArgumentException("ApplicationKey is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.ApplicationSecret))
            throw new ArgumentException("ApplicationSecret is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.ConsumerKey))
            throw new ArgumentException("ConsumerKey is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.OkmsId))
            throw new ArgumentException("OkmsId is required.", nameof(options));

        _client = new OVHOAuthHttpClient(options);
    }

    internal OVHSecretProvider(IOVHApiClient client) =>
        _client = client;

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.FindSecretByNameAsync(key, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            var secretMetadata = await _client.FindSecretByNameAsync(key, cancellationToken);
            var currentVersion = secretMetadata is not null
                ? GetCurrentVersion(secretMetadata, query?.Version)
                : 1;

            var access = await _client.AccessVersionAsync(key, currentVersion, cancellationToken);
            var value = access.Data;

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = access.Id.ToString(),
                CreatedAt = secret.CreatedAt,
                UpdatedAt = secret.UpdatedAt
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"OVH error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _client.FindSecretByNameAsync(key, cancellationToken);

            if (existing is null)
            {
                var created = await _client.CreateSecretAsync(key, value, cancellationToken);

                return new SecretValue
                {
                    Key = key,
                    Value = value,
                    Version = "1",
                    CreatedAt = created.CreatedAt,
                    UpdatedAt = created.UpdatedAt
                };
            }
            else
            {
                var version = await _client.CreateVersionAsync(key, value, cancellationToken);

                return new SecretValue
                {
                    Key = key,
                    Value = value,
                    Version = version.Id.ToString(),
                    CreatedAt = existing.CreatedAt,
                    UpdatedAt = existing.UpdatedAt
                };
            }
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"OVH error putting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteSecretAsync(key, cancellationToken);
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"OVH error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var versions = await _client.ListVersionsAsync(key, cancellationToken);

            var latest = versions.OrderByDescending(v => v.Id).FirstOrDefault();

            return versions
                .Select(v => new SecretVersionInfo
                {
                    Version = v.Id.ToString(),
                    CreatedAt = v.CreatedAt,
                    IsCurrent = v.Id == latest?.Id && v.State == "ACTIVE"
                })
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"OVH error listing versions for '{key}': {ex.Message}", ex);
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
                $"OVH error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }

    private static int GetCurrentVersion(OVHSecretResult secret, string? requestedVersion)
    {
        if (int.TryParse(requestedVersion, out var parsed))
            return parsed;

        return 1;
    }
}
