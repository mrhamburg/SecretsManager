using SecretsManager.Builder;

namespace SecretsManager.GoogleSecretManager;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithGoogleSecretManager(
        this SecretProviderBuilder builder,
        Action<GoogleSecretManagerOptions>? configure = null)
    {
        builder.RegisterProvider(new GoogleSecretManagerSecretProviderFactory());

        if (configure is null) return builder;

        var options = new GoogleSecretManagerOptions();
        configure(options);

        builder.UseProvider("googlesecretmanager", settings =>
        {
            settings["project.id"] = options.ProjectId;

            if (options.CredentialsPath is not null)
                settings["credentials.path"] = options.CredentialsPath;

            if (options.CredentialsJson is not null)
                settings["credentials.json"] = options.CredentialsJson;

            if (options.Endpoint is not null)
                settings["endpoint"] = options.Endpoint;
        });

        return builder;
    }
}
