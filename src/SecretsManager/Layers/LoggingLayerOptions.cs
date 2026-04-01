using System;

namespace SecretsManager;

/// <summary>
/// Options for <see cref="LoggingLayer"/>.
/// </summary>
public sealed class LoggingLayerOptions
{
    /// <summary>
    /// Logging sink implementation. Defaults to <see cref="NullSecretProviderLogger"/>.
    /// </summary>
    public ISecretProviderLogger Logger { get; set; } = new NullSecretProviderLogger();

    /// <summary>
    /// Optional operation name prefix for logs.
    /// </summary>
    public string OperationPrefix { get; set; } = string.Empty;
}

/// <summary>
/// Logging hook used by <see cref="LoggingLayer"/>.
/// </summary>
public interface ISecretProviderLogger
{
    /// <summary>
    /// Called immediately before a provider operation runs.
    /// </summary>
    void LogStart(SecretProviderLogContext context);

    /// <summary>
    /// Called when an operation finishes successfully.
    /// </summary>
    void LogSuccess(SecretProviderLogContext context, TimeSpan duration);

    /// <summary>
    /// Called when an operation throws.
    /// </summary>
    void LogFailure(SecretProviderLogContext context, Exception exception, TimeSpan duration);
}

/// <summary>
/// Context object passed to logging sinks.
/// </summary>
public sealed class SecretProviderLogContext
{
    public string Operation { get; init; } = string.Empty;
    public string? Key { get; init; }
    public bool IsMetadataWrite { get; init; }
}

/// <summary>
/// Default no-op logger used by default.
/// </summary>
public sealed class NullSecretProviderLogger : ISecretProviderLogger
{
    public void LogStart(SecretProviderLogContext context)
    {
    }

    public void LogSuccess(SecretProviderLogContext context, TimeSpan duration)
    {
    }

    public void LogFailure(SecretProviderLogContext context, Exception exception, TimeSpan duration)
    {
    }
}
