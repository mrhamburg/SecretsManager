using System;

namespace SecretsManager;

/// <summary>
/// Options for <see cref="OtelMetricsLayer"/>.
/// </summary>
public sealed class OtelMetricsLayerOptions
{
    /// <summary>
    /// Meter name for emitted metrics.
    /// </summary>
    public string MeterName { get; set; } = "SecretsManager";

    /// <summary>
    /// Meter version.
    /// </summary>
    public string MeterVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Prefix for metric names.
    /// </summary>
    public string MetricPrefix { get; set; } = "secrets.";
}
