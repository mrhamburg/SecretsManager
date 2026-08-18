using SecretsManager.Conjur.Internal;
using SecretsManager.Internal;

namespace SecretsManager.Conjur;

public sealed class ConjurSecretProvider : ISecretProvider
{
    private readonly IConjurApiClient _client;

    public ConjurSecretProvider(ConjurOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Url))
            throw new ArgumentException("Url is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.Account))
            throw new ArgumentException("Account is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.Login))
            throw new ArgumentException("Login is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("ApiKey is required.", nameof(options));

        _client = new ConjurApiClient(options);
    }

    internal ConjurSecretProvider(IConjurApiClient client) =>
        _client = client;

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            int? version = null;
            if (query?.Version is { } v && int.TryParse(v, out var parsed))
                version = parsed;

            var value = await _client.GetSecretValueAsync(key, version, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            var resolved = version.HasValue
                ? await FindVersionAsync(key, version.Value, cancellationToken)
                : await LatestVersionAsync(key, cancellationToken);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = resolved?.Version.ToString(),
                CreatedAt = resolved?.CreatedAt
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Conjur error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.PutSecretAsync(key, value, cancellationToken);

            var latest = await LatestVersionAsync(key, cancellationToken);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = latest?.Version.ToString(),
                CreatedAt = latest?.CreatedAt
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"Conjur error putting secret '{key}': {ex.Message}", ex);
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
                $"Conjur error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var versions = await _client.ListVersionsAsync(key, cancellationToken);

            return versions
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
                $"Conjur error listing versions for '{key}': {ex.Message}", ex);
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
                $"Conjur error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    private async Task<ConjurVersionResult?> LatestVersionAsync(string key, CancellationToken cancellationToken)
    {
        var versions = await _client.ListVersionsAsync(key, cancellationToken);
        return versions.OrderByDescending(v => v.Version).FirstOrDefault();
    }

    private async Task<ConjurVersionResult?> FindVersionAsync(
        string key, int version, CancellationToken cancellationToken)
    {
        var versions = await _client.ListVersionsAsync(key, cancellationToken);
        return versions.FirstOrDefault(v => v.Version == version);
    }

    public async ValueTask DisposeAsync() =>
        await _client.DisposeAsync();
}