namespace SecretsManager.PostgreSql.Internal;

internal record PostgreSqlSecretRow(
    int Id,
    string Key,
    string? ContentType,
    IReadOnlyDictionary<string, string>? Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

internal record PostgreSqlVersionRow(
    int Id,
    int SecretId,
    int Version,
    string Value,
    bool Encrypted,
    DateTimeOffset CreatedAt,
    bool IsCurrent);

internal interface IPostgreSqlSecretStore : IAsyncDisposable
{
    Task EnsureSchemaAsync(CancellationToken cancellationToken);

    Task<PostgreSqlSecretRow?> GetSecretByKeyAsync(string key, CancellationToken cancellationToken);

    Task<PostgreSqlVersionRow?> GetVersionAsync(int secretId, int? version, CancellationToken cancellationToken);

    Task<(PostgreSqlSecretRow Secret, PostgreSqlVersionRow Version)> PutSecretAsync(
        string key, string value, bool encrypted, string? contentType,
        IReadOnlyDictionary<string, string>? tags, CancellationToken cancellationToken);

    Task<bool> DeleteSecretAsync(string key, CancellationToken cancellationToken);

    Task<List<PostgreSqlVersionRow>> ListVersionsAsync(int secretId, CancellationToken cancellationToken);

    Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken);
}
