namespace SecretsManager.AliyunKms.Tests;

public sealed class AliyunKmsSecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsAliyunKms()
    {
        var factory = new AliyunKmsSecretProviderFactory();
        Assert.Equal("aliyunKms", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithAllFields_SetsAllOptions()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "cn-hangzhou",
            ["access.key"] = "LTAI5t...",
            ["secret.key"] = "abc123...",
            ["endpoint"] = "kms-custom.aliyuncs.com"
        };

        var options = AliyunKmsSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("cn-hangzhou", options.Region);
        Assert.Equal("LTAI5t...", options.AccessKeyId);
        Assert.Equal("abc123...", options.AccessKeySecret);
        Assert.Equal("kms-custom.aliyuncs.com", options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = AliyunKmsSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.Region);
        Assert.Equal("", options.AccessKeyId);
        Assert.Equal("", options.AccessKeySecret);
        Assert.Null(options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "  ",
            ["access.key"] = "  ",
            ["secret.key"] = "  ",
            ["endpoint"] = "  "
        };

        var options = AliyunKmsSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.Region);
        Assert.Equal("", options.AccessKeyId);
        Assert.Equal("", options.AccessKeySecret);
        Assert.Null(options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_PartialConfig_SetsOnlyProvidedFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "ap-southeast-1",
            ["access.key"] = "AK123"
        };

        var options = AliyunKmsSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("ap-southeast-1", options.Region);
        Assert.Equal("AK123", options.AccessKeyId);
        Assert.Equal("", options.AccessKeySecret);
        Assert.Null(options.Endpoint);
    }
}
