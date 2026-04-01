namespace SecretsManager;

/// <summary>
/// Decorates an existing provider with additional capabilities.
/// </summary>
public interface ISecretProviderLayer
{
    /// <summary>
    /// Wraps the given provider with a concrete layer implementation.
    /// </summary>
    ISecretProvider Wrap(ISecretProvider inner);
}
