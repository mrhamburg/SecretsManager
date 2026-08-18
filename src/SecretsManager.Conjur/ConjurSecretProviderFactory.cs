using SecretsManager.Builder;

namespace SecretsManager.Conjur;

public sealed class ConjurSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "conjur";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new ConjurSecretProvider(options);
    }

    internal static ConjurOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new ConjurOptions();

        if (configuration.TryGetValue("url", out var url) && !string.IsNullOrWhiteSpace(url))
            options.Url = url;

        if (configuration.TryGetValue("account", out var account) && !string.IsNullOrWhiteSpace(account))
            options.Account = account;

        if (configuration.TryGetValue("login", out var login) && !string.IsNullOrWhiteSpace(login))
            options.Login = login;

        if (configuration.TryGetValue("apikey", out var apiKey) && !string.IsNullOrWhiteSpace(apiKey))
            options.ApiKey = apiKey;

        if (configuration.TryGetValue("policy.path", out var policyPath) && !string.IsNullOrWhiteSpace(policyPath))
            options.PolicyPath = policyPath;

        if (configuration.TryGetValue("skip.tls.verify", out var skipVerify) && !string.IsNullOrWhiteSpace(skipVerify))
            options.SkipTlsVerify = bool.TryParse(skipVerify, out var parsed) && parsed;

        return options;
    }
}