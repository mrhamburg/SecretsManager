namespace SecretsManager.Builder;

/// <summary>
/// Factory interface that each provider package implements to create its <see cref="ISecretProvider"/>
/// from a flat configuration dictionary.
/// </summary>
public interface ISecretProviderFactory
{
    /// <summary>
    /// The provider name used in YAML, environment variables, and the builder API (e.g. "filesystem").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Creates an <see cref="ISecretProvider"/> from the given configuration settings.
    /// Settings use dot-separated keys for nesting (e.g. "encryption.key").
    /// </summary>
    ISecretProvider Create(IReadOnlyDictionary<string, string> configuration);
}
