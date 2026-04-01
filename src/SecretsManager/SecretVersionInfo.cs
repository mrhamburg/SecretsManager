namespace SecretsManager;

/// <summary>
/// Describes a single version of a secret without exposing its value.
/// </summary>
public record SecretVersionInfo
{
    public required string Version { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public bool IsCurrent { get; init; }
}
