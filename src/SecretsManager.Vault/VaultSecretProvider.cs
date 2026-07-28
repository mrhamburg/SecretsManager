using SecretsManager.Internal;
using SecretsManager.Vault.Internal;

namespace SecretsManager.Vault;

public sealed class VaultSecretProvider : ISecretProvider
{
    private readonly IVaultApiClient _client;

    public VaultSecretProvider(VaultOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Url))
            throw new ArgumentException("Url is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.Token))
            throw new ArgumentException("Token is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.MountPath))
            throw new ArgumentException("MountPath is required.", nameof(options));

        _client = new VaultApiClient(options);
    }

    internal VaultSecretProvider(IVaultApiClient client) =>
        _client = client;

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            int? version = null;
            if (query?.Version is { } v && int.TryParse(v, out var parsed))
                version = parsed;

            var secret = await _client.GetSecretAsync(key, version, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            var value = secret.Value;

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = secret.Version.ToString(),
                CreatedAt = secret.CreatedAt,
                UpdatedAt = secret.UpdatedAt
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Vault error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.PutSecretAsync(key, value, cancellationToken);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.Version.ToString(),
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Vault error putting secret '{key}': {ex.Message}", ex);
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
                $"Vault error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var versions = await _client.ListVersionsAsync(key, cancellationToken);

            return versions
                .Where(v => !v.Destroyed)
                .Select(v => new SecretVersionInfo
                {
                    Version = v.Version.ToString(),
                    CreatedAt = v.CreatedAt,
                    IsCurrent = v.IsCurrent
                })
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Vault error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.SecretExistsAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new SecretProviderException(
                $"Vault error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}
