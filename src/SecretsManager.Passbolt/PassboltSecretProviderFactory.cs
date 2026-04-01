using SecretsManager.Builder;

namespace SecretsManager.Passbolt;

public sealed class PassboltSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "passbolt";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new PassboltSecretProvider(options);
    }

    internal static PassboltOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new PassboltOptions();

        if (configuration.TryGetValue("base.url", out var baseUrl) && !string.IsNullOrWhiteSpace(baseUrl))
            options.BaseUrl = baseUrl;

        if (configuration.TryGetValue("user.private.key", out var privateKey) && !string.IsNullOrWhiteSpace(privateKey))
            options.UserPrivateKey = privateKey;

        if (configuration.TryGetValue("user.private.passphrase", out var passphrase) && !string.IsNullOrWhiteSpace(passphrase))
            options.UserPrivateKeyPassphrase = passphrase;

        if (configuration.TryGetValue("user.key.fingerprint", out var fingerprint) && !string.IsNullOrWhiteSpace(fingerprint))
            options.UserKeyFingerprint = fingerprint;

        return options;
    }
}
