using SecretsManager.Builder;

namespace SecretsManager.OVH.Tests;

public sealed class OVHSecretProviderFactoryTests
{
    [Fact]
    public void Create_ReturnsOVHSecretProvider()
    {
        var factory = new OVHSecretProviderFactory();
        var configuration = new Dictionary<string, string>
        {
            ["endpoint"] = "ovh-eu",
            ["application.key"] = "test-app-key",
            ["application.secret"] = "test-app-secret",
            ["consumer.key"] = "test-consumer-key",
            ["okms.id"] = "test-okms-id"
        };

        var provider = factory.Create(configuration);

        Assert.IsType<OVHSecretProvider>(provider);
    }

    [Fact]
    public void MapConfiguration_MapsAllValues()
    {
        var configuration = new Dictionary<string, string>
        {
            ["endpoint"] = "ovh-us",
            ["application.key"] = "app-key-123",
            ["application.secret"] = "app-secret-456",
            ["consumer.key"] = "consumer-key-789",
            ["okms.id"] = "okms-12345"
        };

        var options = OVHSecretProviderFactory.MapConfiguration(configuration);

        Assert.Equal("ovh-us", options.Endpoint);
        Assert.Equal("app-key-123", options.ApplicationKey);
        Assert.Equal("app-secret-456", options.ApplicationSecret);
        Assert.Equal("consumer-key-789", options.ConsumerKey);
        Assert.Equal("okms-12345", options.OkmsId);
    }

    [Fact]
    public void MapConfiguration_HandlesMissingValues()
    {
        var configuration = new Dictionary<string, string>();

        var options = OVHSecretProviderFactory.MapConfiguration(configuration);

        Assert.Equal("", options.Endpoint);
        Assert.Equal("", options.ApplicationKey);
        Assert.Equal("", options.ApplicationSecret);
        Assert.Equal("", options.ConsumerKey);
        Assert.Equal("", options.OkmsId);
    }

    [Fact]
    public void MapConfiguration_IgnoresEmptyValues()
    {
        var configuration = new Dictionary<string, string>
        {
            ["endpoint"] = "",
            ["application.key"] = "   ",
            ["application.secret"] = "app-secret",
            ["consumer.key"] = "",
            ["okms.id"] = "okms-id"
        };

        var options = OVHSecretProviderFactory.MapConfiguration(configuration);

        Assert.Equal("", options.Endpoint);
        Assert.Equal("", options.ApplicationKey);
        Assert.Equal("app-secret", options.ApplicationSecret);
        Assert.Equal("", options.ConsumerKey);
        Assert.Equal("okms-id", options.OkmsId);
    }
}