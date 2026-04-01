namespace SecretsManager.Passbolt;

public sealed class PassboltOptions
{
    public string BaseUrl { get; set; } = "";

    public string UserPrivateKey { get; set; } = "";

    public string UserPrivateKeyPassphrase { get; set; } = "";

    public string UserKeyFingerprint { get; set; } = "";
}
