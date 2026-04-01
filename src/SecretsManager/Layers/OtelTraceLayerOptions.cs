using System;

namespace SecretsManager;

/// <summary>
/// Options for <see cref="OtelTraceLayer"/>.
/// </summary>
public sealed class OtelTraceLayerOptions
{
    /// <summary>
    /// ActivitySource name.
    /// </summary>
    public string ActivitySourceName { get; set; } = "SecretsManager";

    /// <summary>
    /// ActivitySource version.
    /// </summary>
    public string ActivitySourceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Whether exception details should be attached to failed activities.
    /// </summary>
    public bool RecordException { get; set; } = true;
}
