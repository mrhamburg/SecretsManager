using SecretsManager.Builder;

namespace SecretsManager.OVH;

public sealed class OVHSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "ovh";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new OVHSecretProvider(options);
    }

    internal static OVHOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new OVHOptions();

        if (configuration.TryGetValue("endpoint", out var endpoint) && !string.IsNullOrWhiteSpace(endpoint))
            options.Endpoint = endpoint;

        if (configuration.TryGetValue("application.key", out var appKey) && !string.IsNullOrWhiteSpace(appKey))
            options.ApplicationKey = appKey;

        if (configuration.TryGetValue("application.secret", out var appSecret) && !string.IsNullOrWhiteSpace(appSecret))
            options.ApplicationSecret = appSecret;

        if (configuration.TryGetValue("consumer.key", out var consumerKey) && !string.IsNullOrWhiteSpace(consumerKey))
            options.ConsumerKey = consumerKey;

        if (configuration.TryGetValue("okms.id", out var okmsId) && !string.IsNullOrWhiteSpace(okmsId))
            options.OkmsId = okmsId;

        return options;
    }
}