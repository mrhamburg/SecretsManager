namespace SecretsManager.TencentCloud.Tests;

public sealed class TencentCloudSecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsTencentCloud()
    {
        var factory = new TencentCloudSecretProviderFactory();
        Assert.Equal("tencentCloud", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithAllFields_SetsAllOptions()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "ap-beijing",
            ["secret.id"] = "AKID123",
            ["secret.key"] = "abc123...",
            ["endpoint"] = "custom.tencentyun.com"
        };

        var options = TencentCloudSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("ap-beijing", options.Region);
        Assert.Equal("AKID123", options.SecretId);
        Assert.Equal("abc123...", options.SecretKey);
        Assert.Equal("custom.tencentyun.com", options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = TencentCloudSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.Region);
        Assert.Equal("", options.SecretId);
        Assert.Equal("", options.SecretKey);
        Assert.Null(options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "  ",
            ["secret.id"] = "  ",
            ["secret.key"] = "  ",
            ["endpoint"] = "  "
        };

        var options = TencentCloudSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.Region);
        Assert.Equal("", options.SecretId);
        Assert.Equal("", options.SecretKey);
        Assert.Null(options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_PartialConfig_SetsOnlyProvidedFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "ap-shanghai",
            ["secret.id"] = "AKID456"
        };

        var options = TencentCloudSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("ap-shanghai", options.Region);
        Assert.Equal("AKID456", options.SecretId);
        Assert.Equal("", options.SecretKey);
        Assert.Null(options.Endpoint);
    }
}
