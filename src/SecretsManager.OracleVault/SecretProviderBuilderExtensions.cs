using SecretsManager.Builder;

namespace SecretsManager.OracleVault;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithOracleVault(
        this SecretProviderBuilder builder,
        Action<OracleVaultOptions>? configure = null)
    {
        builder.RegisterProvider(new OracleVaultSecretProviderFactory());

        if (configure is null) return builder;

        var options = new OracleVaultOptions();
        configure(options);

        builder.UseProvider("oraclevault", settings =>
        {
            if (options.AuthenticationType is not "configfile")
                settings["authentication.type"] = options.AuthenticationType;

            if (options.ProfileName is not null)
                settings["authentication.profilename"] = options.ProfileName;

            if (options.ConfigFilePath is not null)
                settings["authentication.configfilepath"] = options.ConfigFilePath;

            if (options.Region is not null)
                settings["region"] = options.Region;

            if (options.VaultId is not null)
                settings["vault.id"] = options.VaultId;

            if (options.CompartmentId is not null)
                settings["compartment.id"] = options.CompartmentId;
        });

        return builder;
    }
}
