using SecretsManager.Builder;

namespace SecretsManager.TencentCloud;

public sealed class TencentCloudSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "tencentCloud";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new TencentCloudSecretProvider(options);
    }

    internal static TencentCloudOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new TencentCloudOptions();

        if (configuration.TryGetValue("region", out var region) && !string.IsNullOrWhiteSpace(region))
            options.Region = region;

        if (configuration.TryGetValue("secret.id", out var secretId) && !string.IsNullOrWhiteSpace(secretId))
            options.SecretId = secretId;

        if (configuration.TryGetValue("secret.key", out var secretKey) && !string.IsNullOrWhiteSpace(secretKey))
            options.SecretKey = secretKey;

        if (configuration.TryGetValue("endpoint", out var endpoint) && !string.IsNullOrWhiteSpace(endpoint))
            options.Endpoint = endpoint;

        return options;
    }
}
