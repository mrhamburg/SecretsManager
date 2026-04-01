namespace SecretsManager.TencentCloud.Internal;

internal record TencentCloudSecretValueResult(
    string SecretId,
    string SecretValue,
    string VersionId,
    DateTimeOffset CreateTime,
    string[]? VersionStages);

internal record TencentCloudSecretResult(
    string SecretId,
    string VersionId);

internal record TencentCloudVersionResult(
    string VersionId,
    DateTimeOffset CreateTime,
    string[]? VersionStages);

internal interface ITencentCloudApiClient : IAsyncDisposable
{
    Task<TencentCloudSecretValueResult?> GetSecretValueAsync(
        string secretName, string? versionId,
        CancellationToken cancellationToken);

    Task<TencentCloudSecretResult> CreateSecretAsync(
        string secretName, string secretValue, string versionId,
        CancellationToken cancellationToken);

    Task<TencentCloudSecretResult> PutSecretValueAsync(
        string secretName, string secretValue, string versionId,
        CancellationToken cancellationToken);

    Task DeleteSecretAsync(
        string secretName, CancellationToken cancellationToken);

    Task<List<TencentCloudVersionResult>> ListSecretVersionIdsAsync(
        string secretName, CancellationToken cancellationToken);
}
