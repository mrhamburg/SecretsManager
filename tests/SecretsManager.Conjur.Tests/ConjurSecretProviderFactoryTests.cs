namespace SecretsManager.Conjur.Tests;

public sealed class ConjurSecretProviderFactoryTests
{
    private static readonly Dictionary<string, string> AllFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["url"] = "https://conjur.example.com:443",
        ["account"] = "myorg",
        ["login"] = "host/app",
        ["apikey"] = "14m9cf91wfsesv1kkhevg12cdywm2wvqy6s8sk53z1ngtazp1t9tykc",
        ["policy.path"] = "dev",
        ["skip.tls.verify"] = "true"
    };

    [Fact]
    public void ProviderName_IsConjur()
    {
        var factory = new ConjurSecretProviderFactory();
        Assert.Equal("conjur", factory.ProviderName);
    }

    [Fact]
    public void Create_ReturnsConjurSecretProvider()
    {
        var provider = new ConjurSecretProviderFactory().Create(AllFields);

        Assert.IsType<ConjurSecretProvider>(provider);
    }

    [Fact]
    public void MapConfiguration_WithAllFields_SetsAllOptions()
    {
        var options = ConjurSecretProviderFactory.MapConfiguration(AllFields);

        Assert.Equal("https://conjur.example.com:443", options.Url);
        Assert.Equal("myorg", options.Account);
        Assert.Equal("host/app", options.Login);
        Assert.Equal("14m9cf91wfsesv1kkhevg12cdywm2wvqy6s8sk53z1ngtazp1t9tykc", options.ApiKey);
        Assert.Equal("dev", options.PolicyPath);
        Assert.True(options.SkipTlsVerify);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var options = ConjurSecretProviderFactory.MapConfiguration(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        Assert.Equal("https://localhost:443", options.Url);
        Assert.Equal("", options.Account);
        Assert.Equal("", options.Login);
        Assert.Equal("", options.ApiKey);
        Assert.Equal("root", options.PolicyPath);
        Assert.False(options.SkipTlsVerify);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["url"] = "  ",
            ["account"] = "  ",
            ["login"] = "  ",
            ["apikey"] = "  ",
            ["policy.path"] = "  ",
            ["skip.tls.verify"] = "  "
        };

        var options = ConjurSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("https://localhost:443", options.Url);
        Assert.Equal("", options.Account);
        Assert.Equal("", options.Login);
        Assert.Equal("", options.ApiKey);
        Assert.Equal("root", options.PolicyPath);
        Assert.False(options.SkipTlsVerify);
    }

    [Fact]
    public void MapConfiguration_PartialConfig_SetsOnlyProvidedFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["url"] = "https://conjur.internal:443",
            ["login"] = "admin",
            ["apikey"] = "some-api-key"
        };

        var options = ConjurSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("https://conjur.internal:443", options.Url);
        Assert.Equal("", options.Account);
        Assert.Equal("admin", options.Login);
        Assert.Equal("some-api-key", options.ApiKey);
        Assert.Equal("root", options.PolicyPath);
        Assert.False(options.SkipTlsVerify);
    }
}