namespace SecretsManager.GoogleSecretManager.Tests;

public sealed class GoogleSecretManagerSecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsGoogleSecretManager()
    {
        var factory = new GoogleSecretManagerSecretProviderFactory();
        Assert.Equal("googlesecretmanager", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithProjectId_SetsProjectId()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["project.id"] = "my-gcp-project"
        };

        var options = GoogleSecretManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("my-gcp-project", options.ProjectId);
    }

    [Fact]
    public void MapConfiguration_WithCredentialsPath_SetsCredentialsPath()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["project.id"] = "my-gcp-project",
            ["credentials.path"] = "/path/to/credentials.json"
        };

        var options = GoogleSecretManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("my-gcp-project", options.ProjectId);
        Assert.Equal("/path/to/credentials.json", options.CredentialsPath);
    }

    [Fact]
    public void MapConfiguration_WithCredentialsJson_SetsCredentialsJson()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["project.id"] = "my-gcp-project",
            ["credentials.json"] = "{\"type\":\"service_account\"}"
        };

        var options = GoogleSecretManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("{\"type\":\"service_account\"}", options.CredentialsJson);
    }

    [Fact]
    public void MapConfiguration_WithEndpoint_SetsEndpoint()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["project.id"] = "my-gcp-project",
            ["endpoint"] = "localhost:8080"
        };

        var options = GoogleSecretManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("localhost:8080", options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_WithAllFields_SetsAllFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["project.id"] = "my-gcp-project",
            ["credentials.path"] = "/path/to/credentials.json",
            ["credentials.json"] = "{\"type\":\"service_account\"}",
            ["endpoint"] = "localhost:8080"
        };

        var options = GoogleSecretManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("my-gcp-project", options.ProjectId);
        Assert.Equal("/path/to/credentials.json", options.CredentialsPath);
        Assert.Equal("{\"type\":\"service_account\"}", options.CredentialsJson);
        Assert.Equal("localhost:8080", options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = GoogleSecretManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.ProjectId);
        Assert.Null(options.CredentialsPath);
        Assert.Null(options.CredentialsJson);
        Assert.Null(options.Endpoint);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["project.id"] = "  ",
            ["credentials.path"] = "  "
        };

        var options = GoogleSecretManagerSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.ProjectId);
        Assert.Null(options.CredentialsPath);
    }
}
