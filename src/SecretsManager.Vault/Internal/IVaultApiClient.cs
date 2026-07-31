namespace SecretsManager.Vault.Internal;

internal record VaultSecretResult(
    string Key,
    string Value,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

internal record VaultVersionResult(
    int Version,
    DateTimeOffset CreatedAt,
    bool IsCurrent,
    bool Destroyed);

internal interface IVaultApiClient : IAsyncDisposable
{
    Task<VaultSecretResult?> GetSecretAsync(string key, int? version, CancellationToken cancellationToken);

    Task<VaultSecretResult> PutSecretAsync(string key, string value, CancellationToken cancellationToken);

    Task DeleteSecretAsync(string key, CancellationToken cancellationToken);

    Task<IReadOnlyList<VaultVersionResult>> ListVersionsAsync(string key, CancellationToken cancellationToken);

    Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken);
}
