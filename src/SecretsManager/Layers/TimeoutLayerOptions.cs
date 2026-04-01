namespace SecretsManager;

/// <summary>
/// Options for <see cref="TimeoutLayer"/>.
/// </summary>
public sealed class TimeoutLayerOptions
{
    /// <summary>
    /// Per-operation timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
