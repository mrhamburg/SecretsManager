using SecretsManager.Builder;

namespace SecretsManager.AliyunKms;

public sealed class AliyunKmsSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "aliyunKms";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new AliyunKmsSecretProvider(options);
    }

    internal static AliyunKmsOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new AliyunKmsOptions();

        if (configuration.TryGetValue("region", out var region) && !string.IsNullOrWhiteSpace(region))
            options.Region = region;

        if (configuration.TryGetValue("access.key", out var accessKey) && !string.IsNullOrWhiteSpace(accessKey))
            options.AccessKeyId = accessKey;

        if (configuration.TryGetValue("secret.key", out var secretKey) && !string.IsNullOrWhiteSpace(secretKey))
            options.AccessKeySecret = secretKey;

        if (configuration.TryGetValue("endpoint", out var endpoint) && !string.IsNullOrWhiteSpace(endpoint))
            options.Endpoint = endpoint;

        return options;
    }
}
