namespace SecretsManager.Passbolt.Internal;

internal record PassboltResourceResult(
    string Id,
    string Name,
    string? Username,
    string? Uri,
    string? Description,
    DateTimeOffset Created,
    DateTimeOffset Modified,
    string ResourceTypeId,
    bool Deleted,
    bool Personal,
    string? EncryptedSecret);

internal record PassboltResourceTypeResult(
    string Id,
    string Slug,
    string Name);

internal interface IPassboltApiClient : IAsyncDisposable
{
    Task EnsureAuthenticatedAsync(CancellationToken cancellationToken);

    Task<List<PassboltResourceResult>> ListResourcesAsync(
        string? search = null, CancellationToken cancellationToken = default);

    Task<PassboltResourceResult?> GetResourceAsync(
        string resourceId, CancellationToken cancellationToken = default);

    Task<PassboltResourceResult> CreateResourceAsync(
        string name, string encryptedSecret, string resourceTypeId,
        string? username, string? uri, CancellationToken cancellationToken = default);

    Task<PassboltResourceResult> UpdateResourceAsync(
        string resourceId, string name, string encryptedSecret, string resourceTypeId,
        string? username, string? uri, CancellationToken cancellationToken = default);

    Task DeleteResourceAsync(
        string resourceId, CancellationToken cancellationToken = default);

    Task<List<PassboltResourceTypeResult>> GetResourceTypesAsync(
        CancellationToken cancellationToken = default);
}
