namespace SecretsManager;

/// <summary>
/// Metadata to attach when creating or updating a secret.
/// </summary>
public record SecretMetadata
{
    /// <summary>
    /// MIME type of the secret value (e.g. "application/json", "text/plain").
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Arbitrary key-value tags for categorization and filtering.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Tags { get; init; }
}
