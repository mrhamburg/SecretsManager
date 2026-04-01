namespace SecretsManager;

/// <summary>
/// Represents a secret retrieved from a backend store.
/// </summary>
public record SecretValue
{
    public required string Key { get; init; }
    public required string Value { get; init; }
    public string? Version { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public string? ContentType { get; init; }
    public IReadOnlyDictionary<string, string>? Tags { get; init; }
}
