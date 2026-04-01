namespace SecretsManager.AzureKeyVault.Tests;

public sealed class AzureKeyVaultSecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsAzureKeyVault()
    {
        var factory = new AzureKeyVaultSecretProviderFactory();
        Assert.Equal("azurekeyvault", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithVaultUrl_SetsVaultUrl()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["vault.url"] = "https://myvault.vault.azure.net"
        };

        var options = AzureKeyVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("https://myvault.vault.azure.net", options.VaultUrl);
    }

    [Fact]
    public void MapConfiguration_WithServicePrincipal_SetsAllFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["vault.url"] = "https://myvault.vault.azure.net",
            ["authentication.type"] = "serviceprincipal",
            ["authentication.tenantid"] = "tenant-123",
            ["authentication.clientid"] = "client-456",
            ["authentication.clientsecret"] = "secret-789"
        };

        var options = AzureKeyVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("https://myvault.vault.azure.net", options.VaultUrl);
        Assert.Equal("serviceprincipal", options.AuthenticationType);
        Assert.Equal("tenant-123", options.TenantId);
        Assert.Equal("client-456", options.ClientId);
        Assert.Equal("secret-789", options.ClientSecret);
    }

    [Fact]
    public void MapConfiguration_WithManagedIdentity_SetsClientId()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["vault.url"] = "https://myvault.vault.azure.net",
            ["authentication.type"] = "managedidentity",
            ["authentication.clientid"] = "user-assigned-id"
        };

        var options = AzureKeyVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("managedidentity", options.AuthenticationType);
        Assert.Equal("user-assigned-id", options.ClientId);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = AzureKeyVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.VaultUrl);
        Assert.Equal("default", options.AuthenticationType);
        Assert.Null(options.TenantId);
        Assert.Null(options.ClientId);
        Assert.Null(options.ClientSecret);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["vault.url"] = "  ",
            ["authentication.type"] = "  "
        };

        var options = AzureKeyVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("", options.VaultUrl);
        Assert.Equal("default", options.AuthenticationType);
    }
}
