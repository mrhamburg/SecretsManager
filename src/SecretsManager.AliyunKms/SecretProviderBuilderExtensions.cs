using SecretsManager.Builder;

namespace SecretsManager.AliyunKms;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithAliyunKms(
        this SecretProviderBuilder builder,
        Action<AliyunKmsOptions>? configure = null)
    {
        builder.RegisterProvider(new AliyunKmsSecretProviderFactory());

        if (configure is null) return builder;

        var options = new AliyunKmsOptions();
        configure(options);

        builder.UseProvider("aliyunKms", settings =>
        {
            if (!string.IsNullOrEmpty(options.Region))
                settings["region"] = options.Region;

            if (!string.IsNullOrEmpty(options.AccessKeyId))
                settings["access.key"] = options.AccessKeyId;

            if (!string.IsNullOrEmpty(options.AccessKeySecret))
                settings["secret.key"] = options.AccessKeySecret;

            if (!string.IsNullOrEmpty(options.Endpoint))
                settings["endpoint"] = options.Endpoint;
        });

        return builder;
    }
}
