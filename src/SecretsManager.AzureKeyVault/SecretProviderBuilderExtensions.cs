using SecretsManager.Builder;

namespace SecretsManager.AzureKeyVault;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithAzureKeyVault(
        this SecretProviderBuilder builder,
        Action<AzureKeyVaultOptions>? configure = null)
    {
        builder.RegisterProvider(new AzureKeyVaultSecretProviderFactory());

        if (configure is null) return builder;
        
        var options = new AzureKeyVaultOptions();
        configure(options);

        builder.UseProvider("azurekeyvault", settings =>
        {
            settings["vault.url"] = options.VaultUrl;

            if (options.AuthenticationType is not "default")
                settings["authentication.type"] = options.AuthenticationType;

            if (options.TenantId is not null)
                settings["authentication.tenantid"] = options.TenantId;

            if (options.ClientId is not null)
                settings["authentication.clientid"] = options.ClientId;

            if (options.ClientSecret is not null)
                settings["authentication.clientsecret"] = options.ClientSecret;
        });

        return builder;
    }
}
