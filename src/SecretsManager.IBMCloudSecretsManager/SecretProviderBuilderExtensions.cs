using SecretsManager.Builder;

namespace SecretsManager.IBMCloudSecretsManager;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithIBMCloudSecretsManager(
        this SecretProviderBuilder builder,
        Action<IBMCloudSecretsManagerOptions>? configure = null)
    {
        builder.RegisterProvider(new IBMCloudSecretsManagerSecretProviderFactory());

        if (configure is null) return builder;

        var options = new IBMCloudSecretsManagerOptions();
        configure(options);

        builder.UseProvider("ibmcloud-secrets-manager", settings =>
        {
            settings["region"] = options.Region;
            settings["instance.id"] = options.InstanceId;
            settings["api.key"] = options.ApiKey;

            if (!string.IsNullOrEmpty(options.ServiceUrl))
                settings["service.url"] = options.ServiceUrl;
        });

        return builder;
    }
}