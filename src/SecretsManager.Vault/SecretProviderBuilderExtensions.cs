using SecretsManager.Builder;

namespace SecretsManager.Vault;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithVault(
        this SecretProviderBuilder builder,
        Action<VaultOptions>? configure = null)
    {
        builder.RegisterProvider(new VaultSecretProviderFactory());

        if (configure is null) return builder;

        var options = new VaultOptions();
        configure(options);

        builder.UseProvider("vault", settings =>
        {
            if (!string.IsNullOrEmpty(options.Url))
                settings["url"] = options.Url;

            if (!string.IsNullOrEmpty(options.Token))
                settings["token"] = options.Token;

            if (!string.IsNullOrEmpty(options.MountPath))
                settings["mount.path"] = options.MountPath;

            if (options.SkipTlsVerify)
                settings["skip.tls.verify"] = "true";
        });

        return builder;
    }
}
