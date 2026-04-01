using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SecretsManager.AzureKeyVault.Internal;
using SecretsManager.Internal;

namespace SecretsManager.AzureKeyVault;

public sealed class AzureKeyVaultSecretProvider : ISecretProvider
{
    private readonly ISecretClientWrapper _client;

    public AzureKeyVaultSecretProvider(AzureKeyVaultOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.VaultUrl))
            throw new ArgumentException("VaultUrl is required.", nameof(options));

        var credential = CreateCredential(options);
        var client = new SecretClient(new Uri(options.VaultUrl), credential);
        _client = new SecretClientWrapper(client);
    }

    internal AzureKeyVaultSecretProvider(ISecretClientWrapper client)
    {
        _client = client;
    }

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.GetSecretAsync(key, query?.Version, cancellationToken);
            var value = result.Value;

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.Version,
                CreatedAt = result.CreatedOn,
                UpdatedAt = result.UpdatedOn,
                ContentType = result.ContentType,
                Tags = result.Tags?.AsReadOnly()
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (RequestFailedException ex)
        {
            throw new SecretProviderException(
                $"Azure Key Vault error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = metadata?.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var result = await _client.SetSecretAsync(
                key, value, metadata?.ContentType, tags, cancellationToken);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.Version,
                CreatedAt = result.CreatedOn,
                UpdatedAt = result.UpdatedOn,
                ContentType = metadata?.ContentType ?? result.ContentType,
                Tags = metadata?.Tags
            };
        }
        catch (RequestFailedException ex)
        {
            throw new SecretProviderException(
                $"Azure Key Vault error putting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteSecretAsync(key, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (RequestFailedException ex)
        {
            throw new SecretProviderException(
                $"Azure Key Vault error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current version to determine IsCurrent flag
            string? currentVersion;
            try
            {
                var current = await _client.GetSecretAsync(key, null, cancellationToken);
                currentVersion = current.Version;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new SecretNotFoundException(key, ex);
            }

            var versions = new List<SecretVersionInfo>();
            await foreach (var sp in _client.GetSecretVersionsAsync(key, cancellationToken))
            {
                versions.Add(new SecretVersionInfo
                {
                    Version = sp.Version,
                    CreatedAt = sp.CreatedOn,
                    IsCurrent = sp.Version == currentVersion
                });
            }

            return versions.AsReadOnly();
        }
        catch (RequestFailedException ex)
        {
            throw new SecretProviderException(
                $"Azure Key Vault error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.GetSecretAsync(key, null, cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
        catch (RequestFailedException ex)
        {
            throw new SecretProviderException(
                $"Azure Key Vault error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }

    private static Azure.Core.TokenCredential CreateCredential(AzureKeyVaultOptions options)
    {
        return options.AuthenticationType.ToLowerInvariant() switch
        {
            "default" or "" => new DefaultAzureCredential(),
            "serviceprincipal" => new ClientSecretCredential(
                options.TenantId ?? throw new ArgumentException("TenantId is required for service principal authentication."),
                options.ClientId ?? throw new ArgumentException("ClientId is required for service principal authentication."),
                options.ClientSecret ?? throw new ArgumentException("ClientSecret is required for service principal authentication.")),
            "managedidentity" => options.ClientId is not null
                ? new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(options.ClientId))
                : new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned),
            "workloadidentity" => new WorkloadIdentityCredential(
                new WorkloadIdentityCredentialOptions
                {
                    TenantId = options.TenantId ?? "",
                    ClientId = options.ClientId ?? ""
                }),
            _ => throw new ArgumentException(
                $"Unknown authentication type '{options.AuthenticationType}'. " +
                "Supported: default, serviceprincipal, managedidentity, workloadidentity.")
        };
    }
}
