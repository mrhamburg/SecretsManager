namespace SecretsManager;

/// <summary>
/// Thrown when a requested secret does not exist in the backend store.
/// </summary>
public class SecretNotFoundException : SecretProviderException
{
    public string Key { get; }

    public SecretNotFoundException(string key)
        : base($"Secret '{key}' was not found.")
    {
        Key = key;
    }

    public SecretNotFoundException(string key, Exception innerException)
        : base($"Secret '{key}' was not found.", innerException)
    {
        Key = key;
    }
}
