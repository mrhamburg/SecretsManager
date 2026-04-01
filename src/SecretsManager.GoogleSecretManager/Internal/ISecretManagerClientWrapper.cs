namespace SecretsManager.GoogleSecretManager.Internal;

internal record SecretManagerSecretResult(
    string Name,
    string Value,
    string Version,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? ContentType,
    IDictionary<string, string>? Labels);

internal record SecretVersionProperties(
    string Version,
    DateTimeOffset? CreatedAt,
    string State);

internal interface ISecretManagerClientWrapper : IAsyncDisposable
{
    Task<SecretManagerSecretResult> AccessSecretVersionAsync(
        string secretId, string? version, CancellationToken cancellationToken);

    Task<SecretManagerSecretResult> AddSecretVersionAsync(
        string secretId, string value, string? contentType, IDictionary<string, string>? labels,
        CancellationToken cancellationToken);

    Task CreateSecretAsync(
        string secretId, string? contentType, IDictionary<string, string>? labels,
        CancellationToken cancellationToken);

    Task DeleteSecretAsync(string secretId, CancellationToken cancellationToken);

    IAsyncEnumerable<SecretVersionProperties> ListSecretVersionsAsync(
        string secretId, CancellationToken cancellationToken);

    Task<bool> SecretExistsAsync(string secretId, CancellationToken cancellationToken);
}
