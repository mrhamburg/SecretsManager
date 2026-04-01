namespace SecretsManager;

/// <summary>
/// Options for querying a specific secret. Allows targeting a particular version
/// or extracting a single property from a structured secret value.
/// </summary>
public record SecretQuery
{
    /// <summary>
    /// The version identifier to retrieve. When null, the current/latest version is returned.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// A property path to extract from a structured (e.g. JSON) secret value.
    /// When null, the full secret value is returned.
    /// </summary>
    public string? Property { get; init; }
}
