namespace SecretsManager.Passbolt.Tests;

public sealed class PassboltSecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsPassbolt()
    {
        var factory = new PassboltSecretProviderFactory();
        Assert.Equal("passbolt", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithAllFields_SetsAllOptions()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["base.url"] = "https://passbolt.example.com",
            ["user.private.key"] = "-----BEGIN PGP PRIVATE KEY BLOCK-----...",
            ["user.private.passphrase"] = "my-passphrase",
            ["user.key.fingerprint"] = "ABCDEF1234567890"
        };

        var options = PassboltSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("https://passbolt.example.com", options.BaseUrl);
        Assert.Equal("-----BEGIN PGP PRIVATE KEY BLOCK-----...", options.UserPrivateKey);
        Assert.Equal("my-passphrase", options.UserPrivateKeyPassphrase);
        Assert.Equal("ABCDEF1234567890", options.UserKeyFingerprint);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = PassboltSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.BaseUrl);
        Assert.Equal("", options.UserPrivateKey);
        Assert.Equal("", options.UserPrivateKeyPassphrase);
        Assert.Equal("", options.UserKeyFingerprint);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["base.url"] = "  ",
            ["user.private.key"] = "  ",
            ["user.private.key.passphrase"] = "  ",
            ["user.key.fingerprint"] = "  "
        };

        var options = PassboltSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.BaseUrl);
        Assert.Equal("", options.UserPrivateKey);
        Assert.Equal("", options.UserPrivateKeyPassphrase);
        Assert.Equal("", options.UserKeyFingerprint);
    }

    [Fact]
    public void MapConfiguration_PartialConfig_SetsOnlyProvidedFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["base.url"] = "https://passbolt.local",
            ["user.key.fingerprint"] = "FINGERPRINT123"
        };

        var options = PassboltSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("https://passbolt.local", options.BaseUrl);
        Assert.Equal("", options.UserPrivateKey);
        Assert.Equal("", options.UserPrivateKeyPassphrase);
        Assert.Equal("FINGERPRINT123", options.UserKeyFingerprint);
    }
}
