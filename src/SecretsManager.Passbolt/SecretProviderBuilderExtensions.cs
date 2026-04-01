using SecretsManager.Builder;

namespace SecretsManager.Passbolt;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithPassbolt(
        this SecretProviderBuilder builder,
        Action<PassboltOptions>? configure = null)
    {
        builder.RegisterProvider(new PassboltSecretProviderFactory());

        if (configure is null) return builder;

        var options = new PassboltOptions();
        configure(options);

        builder.UseProvider("passbolt", settings =>
        {
            if (!string.IsNullOrEmpty(options.BaseUrl))
                settings["base.url"] = options.BaseUrl;

            if (!string.IsNullOrEmpty(options.UserPrivateKey))
                settings["user.private.key"] = options.UserPrivateKey;

            if (!string.IsNullOrEmpty(options.UserPrivateKeyPassphrase))
                settings["user.private.passphrase"] = options.UserPrivateKeyPassphrase;

            if (!string.IsNullOrEmpty(options.UserKeyFingerprint))
                settings["user.key.fingerprint"] = options.UserKeyFingerprint;
        });

        return builder;
    }
}
