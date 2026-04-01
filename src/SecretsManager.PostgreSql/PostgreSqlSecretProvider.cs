using System.Text;
using System.Text.Json;
using SecretsManager.Internal;
using SecretsManager.PostgreSql.Internal;

namespace SecretsManager.PostgreSql;

public sealed class PostgreSqlSecretProvider : ISecretProvider
{
    private readonly IPostgreSqlSecretStore _store;
    private readonly ISecretEncryptor _encryptor;

    public PostgreSqlSecretProvider(PostgreSqlOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("ConnectionString is required.", nameof(options));

        _store = new PostgreSqlSecretStore(options);
        _encryptor = CreateEncryptor(options);
    }

    internal PostgreSqlSecretProvider(IPostgreSqlSecretStore store, ISecretEncryptor encryptor)
    {
        _store = store;
        _encryptor = encryptor;
    }

    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _store.GetSecretByKeyAsync(key, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            int? versionNumber = null;
            if (query?.Version is { } versionStr)
            {
                if (!int.TryParse(versionStr, out var parsed))
                    throw new SecretProviderException(
                        $"Invalid version '{versionStr}' for secret '{key}': version must be a positive integer.");
                versionNumber = parsed;
            }

            var version = await _store.GetVersionAsync(secret.Id, versionNumber, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            var value = DecryptValue(version);

            if (query?.Property is { } property)
                value = JsonPropertyExtractor.Extract(value, property);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = version.Version.ToString(),
                CreatedAt = secret.CreatedAt,
                UpdatedAt = secret.UpdatedAt,
                ContentType = secret.ContentType,
                Tags = secret.Tags
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"PostgreSQL error for secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var storedValue = EncryptValue(value, out var encrypted);

            var (secret, version) = await _store.PutSecretAsync(
                key, storedValue, encrypted,
                metadata?.ContentType, metadata?.Tags, cancellationToken);

            return new SecretValue
            {
                Key = key,
                Value = value,
                Version = version.Version.ToString(),
                CreatedAt = version.CreatedAt,
                UpdatedAt = secret.UpdatedAt,
                ContentType = secret.ContentType,
                Tags = secret.Tags
            };
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"PostgreSQL error putting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _store.DeleteSecretAsync(key, cancellationToken);
            if (!deleted)
                throw new SecretNotFoundException(key);
        }
        catch (Exception ex) when (ex is not SecretProviderException)
        {
            throw new SecretProviderException(
                $"PostgreSQL error deleting secret '{key}': {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _store.GetSecretByKeyAsync(key, cancellationToken)
                ?? throw new SecretNotFoundException(key);

            var versions = await _store.ListVersionsAsync(secret.Id, cancellationToken);

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
                $"PostgreSQL error listing versions for '{key}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _store.SecretExistsAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new SecretProviderException(
                $"PostgreSQL error checking existence of '{key}': {ex.Message}", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _encryptor.Dispose();
        await _store.DisposeAsync();
    }

    private string EncryptValue(string value, out bool encrypted)
    {
        if (!_encryptor.IsEnabled)
        {
            encrypted = false;
            return value;
        }

        var payload = _encryptor.Encrypt(Encoding.UTF8.GetBytes(value));
        encrypted = true;
        return JsonSerializer.Serialize(new
        {
            n = Convert.ToBase64String(payload.Nonce),
            c = Convert.ToBase64String(payload.Ciphertext),
            t = Convert.ToBase64String(payload.Tag)
        });
    }

    private string DecryptValue(PostgreSqlVersionRow version)
    {
        if (!version.Encrypted)
            return version.Value;

        using var doc = JsonDocument.Parse(version.Value);
        var root = doc.RootElement;

        var payload = new EncryptedPayload(
            Convert.FromBase64String(root.GetProperty("n").GetString()!),
            Convert.FromBase64String(root.GetProperty("c").GetString()!),
            Convert.FromBase64String(root.GetProperty("t").GetString()!));

        return Encoding.UTF8.GetString(_encryptor.Decrypt(payload));
    }

    private static ISecretEncryptor CreateEncryptor(PostgreSqlOptions options)
    {
        if (options.EncryptionKey is { } base64Key)
            return new AesGcmEncryptor(Convert.FromBase64String(base64Key));

        if (options.EncryptionKeyFile is { } keyFilePath)
        {
            var raw = File.ReadAllBytes(keyFilePath);
            var key = raw.Length == 32 ? raw : Convert.FromBase64String(Encoding.UTF8.GetString(raw).Trim());
            return new AesGcmEncryptor(key);
        }

        return new NullEncryptor();
    }
}
