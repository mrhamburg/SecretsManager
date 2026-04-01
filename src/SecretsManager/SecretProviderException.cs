namespace SecretsManager;

/// <summary>
/// Base exception for errors originating from a secret provider backend.
/// </summary>
public class SecretProviderException : Exception
{
    public SecretProviderException(string message)
        : base(message) { }

    public SecretProviderException(string message, Exception innerException)
        : base(message, innerException) { }
}
