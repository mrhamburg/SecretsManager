namespace SecretsManager.IBMCloudSecretsManager.Internal;

internal record IBMCloudSecretsManagerSecretResult(
    string Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string[] Tags,
    int VersionCount);

internal record IBMCloudSecretsManagerVersionResult(
    int Revision,
    string SecretId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool Latest);

internal record IBMCloudSecretsManagerAccessResult(
    string SecretId,
    int Revision,
    string Data);

internal interface IIBMCloudSecretsManagerApiClient : IAsyncDisposable
{
    Task<IBMCloudSecretsManagerSecretResult?> FindSecretByNameAsync(string name, CancellationToken cancellationToken);

    Task<IBMCloudSecretsManagerSecretResult> CreateSecretAsync(
        string name, string[]? tags, CancellationToken cancellationToken);

    Task DeleteSecretAsync(string secretId, CancellationToken cancellationToken);

    Task<IBMCloudSecretsManagerVersionResult> CreateVersionAsync(
        string secretId, string data, CancellationToken cancellationToken);

    Task<IBMCloudSecretsManagerAccessResult> AccessVersionAsync(
        string secretId, string revision, CancellationToken cancellationToken);

    Task<List<IBMCloudSecretsManagerVersionResult>> ListVersionsAsync(
        string secretId, CancellationToken cancellationToken);
}