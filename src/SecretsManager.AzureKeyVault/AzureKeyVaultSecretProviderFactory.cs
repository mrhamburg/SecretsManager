using SecretsManager.Builder;

namespace SecretsManager.AzureKeyVault;

public sealed class AzureKeyVaultSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "azurekeyvault";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new AzureKeyVaultSecretProvider(options);
    }

    internal static AzureKeyVaultOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new AzureKeyVaultOptions();

        if (configuration.TryGetValue("vault.url", out var vaultUrl) && !string.IsNullOrWhiteSpace(vaultUrl))
            options.VaultUrl = vaultUrl;

        if (configuration.TryGetValue("authentication.type", out var authType) && !string.IsNullOrWhiteSpace(authType))
            options.AuthenticationType = authType;

        if (configuration.TryGetValue("authentication.tenantid", out var tenantId) && !string.IsNullOrWhiteSpace(tenantId))
            options.TenantId = tenantId;

        if (configuration.TryGetValue("authentication.clientid", out var clientId) && !string.IsNullOrWhiteSpace(clientId))
            options.ClientId = clientId;

        if (configuration.TryGetValue("authentication.clientsecret", out var clientSecret) && !string.IsNullOrWhiteSpace(clientSecret))
            options.ClientSecret = clientSecret;

        return options;
    }
}
