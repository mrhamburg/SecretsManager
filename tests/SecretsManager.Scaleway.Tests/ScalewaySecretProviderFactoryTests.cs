namespace SecretsManager.Scaleway.Tests;

public sealed class ScalewaySecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsScaleway()
    {
        var factory = new ScalewaySecretProviderFactory();
        Assert.Equal("scaleway", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithAllFields_SetsAllOptions()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "fr-par",
            ["project.id"] = "11111111-1111-1111-1111-111111111111",
            ["access.key"] = "SCWXXXXXXXXXXXXXXXXX",
            ["secret.key"] = "22222222-2222-2222-2222-222222222222"
        };

        var options = ScalewaySecretProviderFactory.MapConfiguration(config);

        Assert.Equal("fr-par", options.Region);
        Assert.Equal("11111111-1111-1111-1111-111111111111", options.ProjectId);
        Assert.Equal("SCWXXXXXXXXXXXXXXXXX", options.AccessKey);
        Assert.Equal("22222222-2222-2222-2222-222222222222", options.SecretKey);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = ScalewaySecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.Region);
        Assert.Equal("", options.ProjectId);
        Assert.Equal("", options.AccessKey);
        Assert.Equal("", options.SecretKey);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "  ",
            ["project.id"] = "  ",
            ["access.key"] = "  ",
            ["secret.key"] = "  "
        };

        var options = ScalewaySecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.Region);
        Assert.Equal("", options.ProjectId);
        Assert.Equal("", options.AccessKey);
        Assert.Equal("", options.SecretKey);
    }

    [Fact]
    public void MapConfiguration_PartialConfig_SetsOnlyProvidedFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "nl-ams",
            ["secret.key"] = "my-secret-key"
        };

        var options = ScalewaySecretProviderFactory.MapConfiguration(config);

        Assert.Equal("nl-ams", options.Region);
        Assert.Equal("", options.ProjectId);
        Assert.Equal("", options.AccessKey);
        Assert.Equal("my-secret-key", options.SecretKey);
    }
}
