using SecretsManager.Builder;

namespace SecretsManager.AwsSecretsManager;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithAwsSecretsManager(
        this SecretProviderBuilder builder,
        Action<AwsSecretsManagerOptions>? configure = null)
    {
        builder.RegisterProvider(new AwsSecretsManagerSecretProviderFactory());

        if (configure is null) return builder;

        var options = new AwsSecretsManagerOptions();
        configure(options);

        builder.UseProvider("awssecretsmanager", settings =>
        {
            settings["region"] = options.Region;

            if (!string.IsNullOrEmpty(options.AccessKey))
                settings["access.key"] = options.AccessKey;

            if (!string.IsNullOrEmpty(options.SecretKey))
                settings["secret.key"] = options.SecretKey;

            if (!string.IsNullOrEmpty(options.SessionToken))
                settings["session.token"] = options.SessionToken;
        });

        return builder;
    }
}
