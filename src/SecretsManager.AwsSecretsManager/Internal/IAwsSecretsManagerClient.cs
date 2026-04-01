namespace SecretsManager.AwsSecretsManager.Internal;

internal record AwsSecretResult(
    string Name,
    string Value,
    string VersionId,
    DateTimeOffset? CreatedDate,
    string? ContentType,
    IReadOnlyDictionary<string, string>? Tags);

internal record AwsSecretVersionInfo(
    string VersionId,
    DateTimeOffset? CreatedDate,
    bool IsCurrent);

internal interface IAwsSecretsManagerClient : IAsyncDisposable
{
    Task<AwsSecretResult> GetSecretValueAsync(
        string secretId, string? versionId, string? versionStage, CancellationToken cancellationToken);

    Task<AwsSecretResult> PutSecretValueAsync(
        string secretId, string value, string? contentType, CancellationToken cancellationToken);

    Task DeleteSecretAsync(string secretId, CancellationToken cancellationToken);

    IAsyncEnumerable<AwsSecretVersionInfo> ListSecretVersionsAsync(
        string secretId, CancellationToken cancellationToken);

    Task<bool> SecretExistsAsync(string secretId, CancellationToken cancellationToken);
}
