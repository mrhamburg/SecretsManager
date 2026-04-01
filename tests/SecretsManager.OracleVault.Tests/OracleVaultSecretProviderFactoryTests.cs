namespace SecretsManager.OracleVault.Tests;

public sealed class OracleVaultSecretProviderFactoryTests
{
    [Fact]
    public void ProviderName_IsOracleVault()
    {
        var factory = new OracleVaultSecretProviderFactory();
        Assert.Equal("oraclevault", factory.ProviderName);
    }

    [Fact]
    public void MapConfiguration_WithRegion_SetsRegion()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "us-phoenix-1"
        };

        var options = OracleVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("us-phoenix-1", options.Region);
    }

    [Fact]
    public void MapConfiguration_WithVaultAndCompartment_SetsAllFields()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["authentication.type"] = "configfile",
            ["authentication.profilename"] = "PROD",
            ["authentication.configfilepath"] = "/path/to/config",
            ["region"] = "eu-frankfurt-1",
            ["vault.id"] = "ocid1.vault.oc1..xxxxx",
            ["compartment.id"] = "ocid1.compartment.oc1..xxxxx"
        };

        var options = OracleVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("configfile", options.AuthenticationType);
        Assert.Equal("PROD", options.ProfileName);
        Assert.Equal("/path/to/config", options.ConfigFilePath);
        Assert.Equal("eu-frankfurt-1", options.Region);
        Assert.Equal("ocid1.vault.oc1..xxxxx", options.VaultId);
        Assert.Equal("ocid1.compartment.oc1..xxxxx", options.CompartmentId);
    }

    [Fact]
    public void MapConfiguration_WithInstancePrincipal_SetsAuthType()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["authentication.type"] = "instanceprincipal",
            ["region"] = "us-phoenix-1"
        };

        var options = OracleVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("instanceprincipal", options.AuthenticationType);
        Assert.Equal("us-phoenix-1", options.Region);
    }

    [Fact]
    public void MapConfiguration_Empty_UsesDefaults()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var options = OracleVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("configfile", options.AuthenticationType);
        Assert.Equal("DEFAULT", options.ProfileName);
        Assert.Null(options.ConfigFilePath);
        Assert.Null(options.Region);
        Assert.Null(options.VaultId);
        Assert.Null(options.CompartmentId);
    }

    [Fact]
    public void MapConfiguration_IgnoresWhitespaceValues()
    {
        var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["region"] = "  ",
            ["authentication.type"] = "  "
        };

        var options = OracleVaultSecretProviderFactory.MapConfiguration(config);

        Assert.Equal("configfile", options.AuthenticationType);
        Assert.Null(options.Region);
    }
}
