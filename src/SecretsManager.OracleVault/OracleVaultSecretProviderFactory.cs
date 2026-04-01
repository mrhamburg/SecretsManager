using SecretsManager.Builder;

namespace SecretsManager.OracleVault;

public sealed class OracleVaultSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "oraclevault";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new OracleVaultSecretProvider(options);
    }

    internal static OracleVaultOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new OracleVaultOptions();

        if (configuration.TryGetValue("authentication.type", out var authType) && !string.IsNullOrWhiteSpace(authType))
            options.AuthenticationType = authType;

        if (configuration.TryGetValue("authentication.profilename", out var profileName) && !string.IsNullOrWhiteSpace(profileName))
            options.ProfileName = profileName;

        if (configuration.TryGetValue("authentication.configfilepath", out var configFilePath) && !string.IsNullOrWhiteSpace(configFilePath))
            options.ConfigFilePath = configFilePath;

        if (configuration.TryGetValue("region", out var region) && !string.IsNullOrWhiteSpace(region))
            options.Region = region;

        if (configuration.TryGetValue("vault.id", out var vaultId) && !string.IsNullOrWhiteSpace(vaultId))
            options.VaultId = vaultId;

        if (configuration.TryGetValue("compartment.id", out var compartmentId) && !string.IsNullOrWhiteSpace(compartmentId))
            options.CompartmentId = compartmentId;

        return options;
    }
}
