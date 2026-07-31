namespace SecretsManager.Vault.Tests;

public sealed class VaultSecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsVault()
    {
        var factory = new VaultSecretProviderFactory();
        Assert.Equal("vault", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithAllFields_SetsAllOptions()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["url"] = "https://vault.example.com:8200",
            ["token"] = "hvs.example-token",
            ["mount.path"] = "my-secrets",
            ["skip.tls.verify"] = "true"
        };

        var options = VaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("https://vault.example.com:8200", options.Url);
        Assert.Equal("hvs.example-token", options.Token);
        Assert.Equal("my-secrets", options.MountPath);
        Assert.True(options.SkipTlsVerify);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = VaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("http://localhost:8200", options.Url);
        Assert.Equal("", options.Token);
        Assert.Equal("secret", options.MountPath);
        Assert.False(options.SkipTlsVerify);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["url"] = "  ",
            ["token"] = "  ",
            ["mount.path"] = "  ",
            ["skip.tls.verify"] = "  "
        };

        var options = VaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("http://localhost:8200", options.Url);
        Assert.Equal("", options.Token);
        Assert.Equal("secret", options.MountPath);
        Assert.False(options.SkipTlsVerify);
    }

    [Fact]
    public void MapConfiguration_PartialConfig_SetsOnlyProvidedFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["url"] = "https://vault.internal:8200",
            ["token"] = "hvs.partial-token"
        };

        var options = VaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("https://vault.internal:8200", options.Url);
        Assert.Equal("hvs.partial-token", options.Token);
        Assert.Equal("secret", options.MountPath);
        Assert.False(options.SkipTlsVerify);
    }
}
