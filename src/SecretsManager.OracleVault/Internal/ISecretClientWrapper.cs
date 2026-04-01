namespace SecretsManager.OracleVault.Internal;

internal record OracleSecretResult(
    string SecretId,
    string Value,
    long? VersionNumber,
    string? VersionName,
    DateTimeOffset? CreatedAt,
    string? ContentType,
    IReadOnlyDictionary<string, string>? Tags);

internal record OracleSecretVersionInfo(
    long VersionNumber,
    string? VersionName,
    DateTimeOffset? CreatedAt,
    bool IsCurrent);

internal record OracleSecretSummary(
    string SecretId,
    string? SecretName);

internal interface ISecretClientWrapper : IAsyncDisposable
{
    Task<OracleSecretResult> GetSecretBundleAsync(string secretId, long? versionNumber, CancellationToken cancellationToken);

    Task<OracleSecretResult> CreateSecretAsync(
        string secretName, string vaultId, string compartmentId, string value,
        string? contentType, IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken);

    Task<OracleSecretResult> UpdateSecretAsync(
        string secretId, long currentVersionNumber, string value,
        string? contentType, IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken);

    Task DeleteSecretAsync(string secretId, CancellationToken cancellationToken);

    IAsyncEnumerable<OracleSecretVersionInfo> GetSecretVersionsAsync(
        string secretId, CancellationToken cancellationToken);

    Task<OracleSecretSummary?> GetSecretSummaryAsync(string secretId, CancellationToken cancellationToken);
}
