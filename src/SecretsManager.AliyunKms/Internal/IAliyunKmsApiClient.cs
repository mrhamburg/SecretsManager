namespace SecretsManager.AliyunKms.Internal;

internal record AliyunSecretValueResult(
    string SecretName,
    string SecretData,
    string VersionId,
    string SecretDataType,
    DateTimeOffset CreateTime,
    string[]? VersionStages);

internal record AliyunSecretResult(
    string SecretName,
    string VersionId,
    string SecretType);

internal record AliyunVersionResult(
    string VersionId,
    DateTimeOffset CreateTime,
    string[]? VersionStages);

internal interface IAliyunKmsApiClient : IAsyncDisposable
{
    Task<AliyunSecretValueResult?> GetSecretValueAsync(
        string secretName, string? versionStage, string? versionId,
        CancellationToken cancellationToken);

    Task<AliyunSecretResult> CreateSecretAsync(
        string secretName, string secretData, string versionId,
        CancellationToken cancellationToken);

    Task<AliyunSecretResult> PutSecretValueAsync(
        string secretName, string secretData, string versionId,
        CancellationToken cancellationToken);

    Task DeleteSecretAsync(
        string secretName, CancellationToken cancellationToken);

    Task<List<AliyunVersionResult>> ListSecretVersionIdsAsync(
        string secretName, CancellationToken cancellationToken);
}
