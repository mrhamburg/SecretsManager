namespace SecretsManager.AwsSecretsManager.Tests;

public sealed class AwsSecretsManagerSecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsAwsSecretsManager()
    {
        var factory = new AwsSecretsManagerSecretProviderFactory();
        Assert.Equal("awssecretsmanager", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithRegion_SetsRegion()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "us-east-1"
        };

        var options = AwsSecretsManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("us-east-1", options.Region);
    }

    [Fact]
    public void MapConfiguration_WithExplicitKeys_SetsAllFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "eu-west-1",
            ["access.key"] = "AKIAIOSFODNN7EXAMPLE",
            ["secret.key"] = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
            ["session.token"] = "FwoGZXIvYXdzE..."
        };

        var options = AwsSecretsManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("eu-west-1", options.Region);
        Assert.Equal("AKIAIOSFODNN7EXAMPLE", options.AccessKey);
        Assert.Equal("wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY", options.SecretKey);
        Assert.Equal("FwoGZXIvYXdzE...", options.SessionToken);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = AwsSecretsManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.Region);
        Assert.Null(options.AccessKey);
        Assert.Null(options.SecretKey);
        Assert.Null(options.SessionToken);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "  ",
            ["access.key"] = "  "
        };

        var options = AwsSecretsManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.Region);
        Assert.Null(options.AccessKey);
    }
}
