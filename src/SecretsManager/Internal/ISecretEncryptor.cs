namespace SecretsManager.Internal;

public record EncryptedPayload(byte[] Nonce, byte[] Ciphertext, byte[] Tag);

public interface ISecretEncryptor : IDisposable
{
    bool IsEnabled { get; }
    EncryptedPayload Encrypt(byte[] plaintext);
    byte[] Decrypt(EncryptedPayload payload);
}
