namespace SecretsManager.Internal;

public sealed class NullEncryptor : ISecretEncryptor
{
    public bool IsEnabled => false;

    public EncryptedPayload Encrypt(byte[] plaintext) =>
        new(Nonce: [], Ciphertext: plaintext, Tag: []);

    public byte[] Decrypt(EncryptedPayload payload) =>
        payload.Ciphertext;

    public void Dispose() { }
}
