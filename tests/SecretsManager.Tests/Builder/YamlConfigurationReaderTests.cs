using SecretsManager.Builder;

namespace SecretsManager.Tests.Builder;

public class YamlConfigurationReaderTests
{
    [Fact]
    public void Read_ValidYaml_ReturnsConfiguration()
    {
        var yaml = """
            provider:
              filesystem:
                path: /secrets
                encryption:
                  key: abc123
            """;

        var config = YamlConfigurationReader.Read(yaml);

        Assert.Equal("filesystem", config.ProviderName);
        Assert.Equal("/secrets", config.Settings["path"]);
        Assert.Equal("abc123", config.Settings["encryption.key"]);
    }

    [Fact]
    public void Read_NestedSettings_AreFlattenedWithDots()
    {
        var yaml = """
            provider:
              vault:
                server: https://vault.example.com
                auth:
                  token:
                    secret: my-token
            """;

        var config = YamlConfigurationReader.Read(yaml);

        Assert.Equal("vault", config.ProviderName);
        Assert.Equal("https://vault.example.com", config.Settings["server"]);
        Assert.Equal("my-token", config.Settings["auth.token.secret"]);
    }

    [Fact]
    public void Read_MissingProviderKey_Throws()
    {
        var yaml = """
            notprovider:
              filesystem:
                path: /secrets
            """;

        var ex = Assert.Throws<InvalidOperationException>(
            () => YamlConfigurationReader.Read(yaml));
        Assert.Contains("provider", ex.Message);
    }

    [Fact]
    public void Read_MultipleProviders_Throws()
    {
        var yaml = """
            provider:
              filesystem:
                path: /a
              vault:
                server: https://b
            """;

        var ex = Assert.Throws<InvalidOperationException>(
            () => YamlConfigurationReader.Read(yaml));
        Assert.Contains("Exactly one provider", ex.Message);
    }

    [Fact]
    public void Read_EmptyYaml_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => YamlConfigurationReader.Read(""));
    }

    [Fact]
    public void Read_ProviderWithNoSettings_ReturnsEmptySettings()
    {
        var yaml = """
            provider:
              filesystem:
            """;

        var config = YamlConfigurationReader.Read(yaml);

        Assert.Equal("filesystem", config.ProviderName);
        Assert.Empty(config.Settings);
    }
}
