using Oci.Common;
using Oci.Common.Auth;
using Oci.Common.Model;
using SecretsManager.Internal;
using SecretsManager.OracleVault.Internal;

namespace SecretsManager.OracleVault;

public sealed class OracleVaultSecretProvider : ISecretProvider
{
    private readonly ISecretClientWrapper _client;
    private readonly OracleVaultOptions _options;

    public OracleVaultSecretProvider(OracleVaultOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;

        if (string.IsNullOrWhiteSpace(options.AuthenticationType))
            throw new ArgumentException("AuthenticationType is required.", nameof(options));

        var authProvider = CreateAuthProvider(options);
        _client = new SecretClientWrapper(
            authProvider,
            options.VaultId ?? string.Empty,
            options.CompartmentId ?? string.Empty,
            null,
            options.Region);
    }

    internal OracleVaultSecretProvider(ISecretClientWrapper client, OracleVaultOptions options)
    {
        _client = client;
        _options = options;
    }

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.GetSecretBundleAsync(key, query?.Version != null ? long.Parse(query.Version) : null, cancellationToken);
            var value = result.Value;

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.VersionNumber?.ToString(),
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.CreatedAt,
                ContentType = result.ContentType,
                Tags = result.Tags
            };
        }
        catch (OciException ex) when (ex.Message.Contains("404") || ex.Message.Contains("not found"))
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (OciException ex)
        {
            throw new SecretProviderException(
                $"Oracle Vault error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = metadata?.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var summary = await _client.GetSecretSummaryAsync(key, cancellationToken);

            OracleSecretResult result;

            if (summary != null)
            {
                var currentVersion = await _client.GetSecretBundleAsync(key, null, cancellationToken);
                result = await _client.UpdateSecretAsync(
                    key,
                    currentVersion.VersionNumber ?? 0,
                    value,
                    metadata?.ContentType,
                    tags,
                    cancellationToken);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_options.VaultId))
                    throw new SecretProviderException("VaultId is required to create a new secret. Configure it in OracleVaultOptions.");

                if (string.IsNullOrWhiteSpace(_options.CompartmentId))
                    throw new SecretProviderException("CompartmentId is required to create a new secret. Configure it in OracleVaultOptions.");

                var secretName = key.Contains('/') ? key.Split('/').Last() : key;
                result = await _client.CreateSecretAsync(
                    secretName,
                    _options.VaultId,
                    _options.CompartmentId,
                    value,
                    metadata?.ContentType,
                    tags,
                    cancellationToken);
            }

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.VersionNumber?.ToString(),
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.CreatedAt,
                ContentType = metadata?.ContentType ?? result.ContentType,
                Tags = metadata?.Tags
            };
        }
        catch (OciException ex) when (ex.Message.Contains("404") || ex.Message.Contains("not found"))
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (OciException ex)
        {
            throw new SecretProviderException(
                $"Oracle Vault error putting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteSecretAsync(key, cancellationToken);
        }
        catch (OciException ex) when (ex.Message.Contains("404") || ex.Message.Contains("not found"))
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (OciException ex)
        {
            throw new SecretProviderException(
                $"Oracle Vault error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _client.GetSecretSummaryAsync(key, cancellationToken);
            if (summary == null)
                throw new SecretNotFoundException(key);

            var versions = new List<SecretVersionInfo>();
            await foreach (var version in _client.GetSecretVersionsAsync(key, cancellationToken))
            {
                versions.Add(new SecretVersionInfo
                {
                    Version = version.VersionNumber.ToString(),
                    CreatedAt = version.CreatedAt,
                    IsCurrent = version.IsCurrent
                });
            }

            return versions.AsReadOnly();
        }
        catch (OciException ex) when (ex.Message.Contains("404") || ex.Message.Contains("not found"))
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (OciException ex)
        {
            throw new SecretProviderException(
                $"Oracle Vault error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _client.GetSecretSummaryAsync(key, cancellationToken);
            return summary != null;
        }
        catch (OciException ex) when (ex.Message.Contains("404") || ex.Message.Contains("not found"))
        {
            return false;
        }
        catch (OciException ex)
        {
            throw new SecretProviderException(
                $"Oracle Vault error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }

    private static IBasicAuthenticationDetailsProvider CreateAuthProvider(OracleVaultOptions options)
    {
        return options.AuthenticationType.ToLowerInvariant() switch
        {
            "configfile" => new ConfigFileAuthenticationDetailsProvider(
                options.ProfileName ?? "DEFAULT",
                options.ConfigFilePath),
            "instanceprincipal" => new InstancePrincipalsAuthenticationDetailsProvider(),
            "securitytoken" => new SessionTokenAuthenticationDetailsProvider(
                options.ConfigFilePath),
            _ => throw new ArgumentException(
                $"Unknown authentication type '{options.AuthenticationType}'. " +
                "Supported: configfile, instanceprincipal, securitytoken.")
        };
    }
}
