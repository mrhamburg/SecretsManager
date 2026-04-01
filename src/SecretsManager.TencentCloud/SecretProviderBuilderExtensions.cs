using SecretsManager.Builder;

namespace SecretsManager.TencentCloud;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithTencentCloud(
        this SecretProviderBuilder builder,
        Action<TencentCloudOptions>? configure = null)
    {
        builder.RegisterProvider(new TencentCloudSecretProviderFactory());

        if (configure is null) return builder;

        var options = new TencentCloudOptions();
        configure(options);

        builder.UseProvider("tencentCloud", settings =>
        {
            if (!string.IsNullOrEmpty(options.Region))
                settings["region"] = options.Region;

            if (!string.IsNullOrEmpty(options.SecretId))
                settings["secret.id"] = options.SecretId;

            if (!string.IsNullOrEmpty(options.SecretKey))
                settings["secret.key"] = options.SecretKey;

            if (!string.IsNullOrEmpty(options.Endpoint))
                settings["endpoint"] = options.Endpoint;
        });

        return builder;
    }
}
