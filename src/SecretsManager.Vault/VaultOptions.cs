namespace SecretsManager.Vault;

public sealed class VaultOptions
{
    public string Url { get; set; } = "http://localhost:8200";

    public string Token { get; set; } = "";

    public string MountPath { get; set; } = "secret";

    public bool SkipTlsVerify { get; set; }
}
