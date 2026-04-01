namespace SecretsManager.AzureKeyVault.Internal;

internal record KeyVaultSecretResult(
    string Name,
    string Value,
    string Version,
    DateTimeOffset? CreatedOn,
    DateTimeOffset? UpdatedOn,
    string? ContentType,
    IDictionary<string, string>? Tags);

internal record SecretVersionProperties(
    string Version,
    DateTimeOffset? CreatedOn,
    bool? Enabled);

internal interface ISecretClientWrapper : IAsyncDisposable
{
    Task<KeyVaultSecretResult> GetSecretAsync(string name, string? version, CancellationToken cancellationToken);

    Task<KeyVaultSecretResult> SetSecretAsync(
        string name, string value, string? contentType, IDictionary<string, string>? tags,
        CancellationToken cancellationToken);

    Task DeleteSecretAsync(string name, CancellationToken cancellationToken);

    IAsyncEnumerable<SecretVersionProperties> GetSecretVersionsAsync(
        string name, CancellationToken cancellationToken);
}
