using SecretsManager.Builder;

namespace SecretsManager.OVH;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithOVH(
        this SecretProviderBuilder builder,
        Action<OVHOptions>? configure = null)
    {
        builder.RegisterProvider(new OVHSecretProviderFactory());

        if (configure is null) return builder;

        var options = new OVHOptions();
        configure(options);

        builder.UseProvider("ovh", settings =>
        {
            settings["endpoint"] = options.Endpoint;

            if (!string.IsNullOrEmpty(options.ApplicationKey))
                settings["application.key"] = options.ApplicationKey;

            if (!string.IsNullOrEmpty(options.ApplicationSecret))
                settings["application.secret"] = options.ApplicationSecret;

            if (!string.IsNullOrEmpty(options.ConsumerKey))
                settings["consumer.key"] = options.ConsumerKey;

            if (!string.IsNullOrEmpty(options.OkmsId))
                settings["okms.id"] = options.OkmsId;
        });

        return builder;
    }
}