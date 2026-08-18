namespace SecretsManager.Conjur.Internal;

internal record ConjurVersionResult(
    int Version,
    DateTimeOffset? CreatedAt,
    bool IsCurrent);

internal interface IConjurApiClient : IAsyncDisposable
{
    Task<string?> GetSecretValueAsync(string key, int? version, CancellationToken cancellationToken);

    Task PutSecretAsync(string key, string value, CancellationToken cancellationToken);

    Task DeleteSecretAsync(string key, CancellationToken cancellationToken);

    Task<IReadOnlyList<ConjurVersionResult>> ListVersionsAsync(string key, CancellationToken cancellationToken);

    Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken);
}