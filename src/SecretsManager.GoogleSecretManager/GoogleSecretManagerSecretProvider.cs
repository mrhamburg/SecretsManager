using Google.Cloud.SecretManager.V1;
using Grpc.Core;
using SecretsManager.GoogleSecretManager.Internal;
using SecretsManager.Internal;

namespace SecretsManager.GoogleSecretManager;

public sealed class GoogleSecretManagerSecretProvider : ISecretProvider
{
    private readonly ISecretManagerClientWrapper _client;

    public GoogleSecretManagerSecretProvider(GoogleSecretManagerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ProjectId))
            throw new ArgumentException("ProjectId is required.", nameof(options));

        var client = CreateClient(options);
        _client = new SecretManagerClientWrapper(client, options.ProjectId);
    }

    internal GoogleSecretManagerSecretProvider(ISecretManagerClientWrapper client)
    {
        _client = client;
    }

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.AccessSecretVersionAsync(key, query?.Version, cancellationToken);
            var value = result.Value;

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = result.Version,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                ContentType = result.ContentType,
                Tags = result.Labels?.AsReadOnly()
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (RpcException ex)
        {
            throw new SecretProviderException(
                $"Google Secret Manager error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var labels = metadata?.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            try
            {
                var result = await _client.AddSecretVersionAsync(
                    key, value, metadata?.ContentType, labels, cancellationToken);

                return new SecretValue
                {
                    Key = key,
                    Value = value,
                    Version = result.Version,
                    CreatedAt = result.CreatedAt,
                    ContentType = metadata?.ContentType,
                    Tags = metadata?.Tags
                };
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                await _client.CreateSecretAsync(key, metadata?.ContentType, labels, cancellationToken);
                var result = await _client.AddSecretVersionAsync(
                    key, value, metadata?.ContentType, labels, cancellationToken);

                return new SecretValue
                {
                    Key = key,
                    Value = value,
                    Version = result.Version,
                    CreatedAt = result.CreatedAt,
                    ContentType = metadata?.ContentType,
                    Tags = metadata?.Tags
                };
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (RpcException ex)
        {
            throw new SecretProviderException(
                $"Google Secret Manager error putting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteSecretAsync(key, cancellationToken);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new SecretNotFoundException(key, ex);
        }
        catch (RpcException ex)
        {
            throw new SecretProviderException(
                $"Google Secret Manager error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            string? currentVersion;
            try
            {
                var current = await _client.AccessSecretVersionAsync(key, "latest", cancellationToken);
                currentVersion = current.Version;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                throw new SecretNotFoundException(key, ex);
            }

            var versions = new List<SecretVersionInfo>();
            await foreach (var v in _client.ListSecretVersionsAsync(key, cancellationToken))
            {
                if (v.State == "ENABLED")
                {
                    versions.Add(new SecretVersionInfo
                    {
                        Version = v.Version,
                        CreatedAt = v.CreatedAt,
                        IsCurrent = v.Version == currentVersion
                    });
                }
            }

            return versions.AsReadOnly();
        }
        catch (RpcException ex)
        {
            throw new SecretProviderException(
                $"Google Secret Manager error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.SecretExistsAsync(key, cancellationToken);
        }
        catch (RpcException ex)
        {
            throw new SecretProviderException(
                $"Google Secret Manager error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }

    private static SecretManagerServiceClient CreateClient(GoogleSecretManagerOptions options)
    {
        var builder = new SecretManagerServiceClientBuilder();

        if (!string.IsNullOrWhiteSpace(options.Endpoint))
            builder.Endpoint = options.Endpoint;

#pragma warning disable CS0618
        if (!string.IsNullOrWhiteSpace(options.CredentialsPath))
            builder.CredentialsPath = options.CredentialsPath;
        else if (!string.IsNullOrWhiteSpace(options.CredentialsJson))
            builder.JsonCredentials = options.CredentialsJson;
#pragma warning restore CS0618

        return builder.Build();
    }
}
