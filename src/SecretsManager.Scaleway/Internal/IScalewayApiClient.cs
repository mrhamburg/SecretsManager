namespace SecretsManager.Scaleway.Internal;

internal record ScalewaySecretResult(
    string Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string[] Tags,
    int VersionCount);

internal record ScalewayVersionResult(
    int Revision,
    string SecretId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool Latest);

internal record ScalewayAccessResult(
    string SecretId,
    int Revision,
    string Data);

internal interface IScalewayApiClient : IAsyncDisposable
{
    Task<ScalewaySecretResult?> FindSecretByNameAsync(string name, CancellationToken cancellationToken);

    Task<ScalewaySecretResult> CreateSecretAsync(
        string name, string[]? tags, CancellationToken cancellationToken);

    Task DeleteSecretAsync(string secretId, CancellationToken cancellationToken);

    Task<ScalewayVersionResult> CreateVersionAsync(
        string secretId, string data, CancellationToken cancellationToken);

    Task<ScalewayAccessResult> AccessVersionAsync(
        string secretId, string revision, CancellationToken cancellationToken);

    Task<List<ScalewayVersionResult>> ListVersionsAsync(
        string secretId, CancellationToken cancellationToken);
}
