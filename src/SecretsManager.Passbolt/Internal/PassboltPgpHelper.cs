using System.IO;
using System.Text;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace SecretsManager.Passbolt.Internal;

internal static class PassboltPgpHelper
{
    public static PgpPublicKey ReadPublicKey(string armoredKey)
    {
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(armoredKey));
        var decoderStream = PgpUtilities.GetDecoderStream(inputStream);
        var factory = new PgpObjectFactory(decoderStream);

        PgpPublicKeyRing? keyRing = null;
        while (keyRing == null)
        {
            var obj = factory.NextPgpObject();
            if (obj is null)
                throw new ArgumentException("No PGP public key found in the provided key data.");
            if (obj is PgpPublicKeyRing kr)
                keyRing = kr;
        }

        foreach (PgpPublicKey key in keyRing.GetPublicKeys())
        {
            if (key.IsEncryptionKey)
                return key;
        }

        throw new ArgumentException("No encryption key found in the PGP public key ring.");
    }

    public static PgpSecretKey ReadSecretKey(string armoredKey)
    {
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(armoredKey));
        var decoderStream = PgpUtilities.GetDecoderStream(inputStream);
        var factory = new PgpObjectFactory(decoderStream);

        PgpSecretKeyRing? keyRing = null;
        while (keyRing == null)
        {
            var obj = factory.NextPgpObject();
            if (obj is null)
                throw new ArgumentException("No PGP secret key found in the provided key data.");
            if (obj is PgpSecretKeyRing kr)
                keyRing = kr;
        }

        foreach (PgpSecretKey key in keyRing.GetSecretKeys())
        {
            if (key.IsSigningKey)
                return key;
        }

        throw new ArgumentException("No signing key found in the PGP secret key ring.");
    }

    public static PgpPrivateKey ExtractPrivateKey(PgpSecretKey secretKey, string passphrase)
    {
        return secretKey.ExtractPrivateKey(passphrase.ToCharArray());
    }

    public static string EncryptAndSign(
        string plaintext,
        PgpPublicKey recipientPublicKey,
        PgpPublicKey senderPublicKey,
        PgpPrivateKey senderPrivateKey)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);

        using var outputStream = new MemoryStream();
        using var compressedStream = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip)
            .Open(outputStream);

        var encryptedDataGenerator = new PgpEncryptedDataGenerator(
            SymmetricKeyAlgorithmTag.Aes256, true, new SecureRandom());
        encryptedDataGenerator.AddMethod(recipientPublicKey);

        using var encryptedStream = encryptedDataGenerator.Open(compressedStream, plainBytes.Length);

        var literalDataGenerator = new PgpLiteralDataGenerator();
        using var literalStream = literalDataGenerator.Open(
            encryptedStream,
            PgpLiteralData.Binary,
            "_CONSOLE",
            plainBytes.Length,
            DateTime.UtcNow);

        literalStream.Write(plainBytes, 0, plainBytes.Length);
        literalStream.Flush();
        literalStream.Close();

        compressedStream.Close();

        var armoredBytes = outputStream.ToArray();
        return Encoding.UTF8.GetString(armoredBytes);
    }

    public static string DecryptAndVerify(
        string armoredMessage,
        PgpSecretKey recipientSecretKey,
        PgpPrivateKey recipientPrivateKey)
    {
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(armoredMessage));
        var decoderStream = PgpUtilities.GetDecoderStream(inputStream);
        var factory = new PgpObjectFactory(decoderStream);

        var encryptedList = factory.NextPgpObject() as PgpEncryptedDataList
            ?? throw new ArgumentException("No encrypted data found in the message.");

        PgpPublicKeyEncryptedData? encryptedData = null;
        foreach (PgpPublicKeyEncryptedData pked in encryptedList.GetEncryptedDataObjects())
        {
            if (pked.KeyId == recipientSecretKey.KeyId)
            {
                encryptedData = pked;
                break;
            }
        }

        if (encryptedData is null)
            throw new ArgumentException("No encrypted data found for the provided key.");

        using var clearStream = encryptedData.GetDataStream(recipientPrivateKey);
        var clearFactory = new PgpObjectFactory(clearStream);
        var message = clearFactory.NextPgpObject();

        if (message is PgpCompressedData compressedData)
        {
            var compressedFactory = new PgpObjectFactory(compressedData.GetDataStream());
            message = compressedFactory.NextPgpObject();
        }

        if (message is PgpLiteralData literalData)
        {
            using var reader = new StreamReader(literalData.GetInputStream(), Encoding.UTF8);
            return reader.ReadToEnd();
        }

        if (message is PgpOnePassSignatureList)
        {
            var nextMessage = clearFactory.NextPgpObject();
            if (nextMessage is PgpLiteralData ld)
            {
                using var reader = new StreamReader(ld.GetInputStream(), Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        throw new ArgumentException("Unexpected message format in decrypted data.");
    }

    public static string GetFingerprint(PgpPublicKey publicKey)
    {
        return BitConverter.ToString(publicKey.GetFingerprint())
            .Replace("-", "")
            .ToUpperInvariant();
    }
}
