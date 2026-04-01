using SecretsManager.Builder;

namespace SecretsManager.Scaleway;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithScaleway(
        this SecretProviderBuilder builder,
        Action<ScalewayOptions>? configure = null)
    {
        builder.RegisterProvider(new ScalewaySecretProviderFactory());

        if (configure is null) return builder;

        var options = new ScalewayOptions();
        configure(options);

        builder.UseProvider("scaleway", settings =>
        {
            settings["region"] = options.Region;
            settings["project.id"] = options.ProjectId;

            if (!string.IsNullOrEmpty(options.AccessKey))
                settings["access.key"] = options.AccessKey;

            if (!string.IsNullOrEmpty(options.SecretKey))
                settings["secret.key"] = options.SecretKey;
        });

        return builder;
    }
}
