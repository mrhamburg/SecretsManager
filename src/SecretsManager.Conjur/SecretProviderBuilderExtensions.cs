using SecretsManager.Builder;

namespace SecretsManager.Conjur;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithConjur(
        this SecretProviderBuilder builder,
        Action<ConjurOptions>? configure = null)
    {
        builder.RegisterProvider(new ConjurSecretProviderFactory());

        if (configure is null) return builder;

        var options = new ConjurOptions();
        configure(options);

        builder.UseProvider("conjur", settings =>
        {
            if (!string.IsNullOrEmpty(options.Url))
                settings["url"] = options.Url;

            if (!string.IsNullOrEmpty(options.Account))
                settings["account"] = options.Account;

            if (!string.IsNullOrEmpty(options.Login))
                settings["login"] = options.Login;

            if (!string.IsNullOrEmpty(options.ApiKey))
                settings["apikey"] = options.ApiKey;

            if (!string.IsNullOrEmpty(options.PolicyPath))
                settings["policy.path"] = options.PolicyPath;

            if (options.SkipTlsVerify)
                settings["skip.tls.verify"] = "true";
        });

        return builder;
    }
}