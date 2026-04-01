using System.Security.Cryptography;

namespace SecretsManager.Internal;

public sealed class AesGcmEncryptor : ISecretEncryptor
{
    private const int NonceSize = 12; // 96 bits, recommended for AES-GCM
    private const int TagSize = 16;   // 128 bits

    private readonly AesGcm _aesGcm;

    public AesGcmEncryptor(byte[] key)
    {
        if (key.Length != 32)
            throw new ArgumentException("AES-256-GCM requires a 256-bit (32-byte) key.", nameof(key));

        _aesGcm = new AesGcm(key, TagSize);
    }

    public bool IsEnabled => true;

    public EncryptedPayload Encrypt(byte[] plaintext)
    {
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        _aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        return new EncryptedPayload(nonce, ciphertext, tag);
    }

    public byte[] Decrypt(EncryptedPayload payload)
    {
        var plaintext = new byte[payload.Ciphertext.Length];
        _aesGcm.Decrypt(payload.Nonce, payload.Ciphertext, payload.Tag, plaintext);
        return plaintext;
    }

    public void Dispose() => _aesGcm.Dispose();
}
