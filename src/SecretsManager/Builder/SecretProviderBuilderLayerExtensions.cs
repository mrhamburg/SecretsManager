using System;

using SecretsManager;

namespace SecretsManager.Builder;

/// <summary>
/// Extension methods for adding operational layers to the provider pipeline.
/// </summary>
public static class SecretProviderBuilderLayerExtensions
{
    /// <summary>
    /// Adds a concurrency-limiting layer.
    /// </summary>
    public static SecretProviderBuilder WithConcurrentLimiter(
        this SecretProviderBuilder builder,
        Action<ConcurrentLimiterLayerOptions>? configure = null)
    {
        var options = new ConcurrentLimiterLayerOptions();
        configure?.Invoke(options);

        builder.AddLayer(new ConcurrentLimiterLayer(options));
        return builder;
    }

    /// <summary>
    /// Adds a retry layer.
    /// </summary>
    public static SecretProviderBuilder WithRetry(
        this SecretProviderBuilder builder,
        Action<RetryLayerOptions>? configure = null)
    {
        var options = new RetryLayerOptions();
        configure?.Invoke(options);

        builder.AddLayer(new RetryLayer(options));
        return builder;
    }

    /// <summary>
    /// Adds a timeout layer.
    /// </summary>
    public static SecretProviderBuilder WithTimeout(
        this SecretProviderBuilder builder,
        Action<TimeoutLayerOptions>? configure = null)
    {
        var options = new TimeoutLayerOptions();
        configure?.Invoke(options);

        builder.AddLayer(new TimeoutLayer(options));
        return builder;
    }

    /// <summary>
    /// Adds a logging layer.
    /// </summary>
    public static SecretProviderBuilder WithLogging(
        this SecretProviderBuilder builder,
        Action<LoggingLayerOptions>? configure = null)
    {
        var options = new LoggingLayerOptions();
        configure?.Invoke(options);

        builder.AddLayer(new LoggingLayer(options));
        return builder;
    }

    /// <summary>
    /// Adds OpenTelemetry metrics layer.
    /// </summary>
    public static SecretProviderBuilder WithOtelMetrics(
        this SecretProviderBuilder builder,
        Action<OtelMetricsLayerOptions>? configure = null)
    {
        var options = new OtelMetricsLayerOptions();
        configure?.Invoke(options);

        builder.AddLayer(new OtelMetricsLayer(options));
        return builder;
    }

    /// <summary>
    /// Adds OpenTelemetry trace layer.
    /// </summary>
    public static SecretProviderBuilder WithOtelTrace(
        this SecretProviderBuilder builder,
        Action<OtelTraceLayerOptions>? configure = null)
    {
        var options = new OtelTraceLayerOptions();
        configure?.Invoke(options);

        builder.AddLayer(new OtelTraceLayer(options));
        return builder;
    }
}
