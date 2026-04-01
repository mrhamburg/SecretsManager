using SecretsManager.Builder;

namespace SecretsManager.IBMCloudSecretsManager;

public sealed class IBMCloudSecretsManagerSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "ibmcloud-secrets-manager";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new IBMCloudSecretsManagerSecretProvider(options);
    }

    internal static IBMCloudSecretsManagerOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new IBMCloudSecretsManagerOptions();

        if (configuration.TryGetValue("region", out var region) && !string.IsNullOrWhiteSpace(region))
            options.Region = region;

        if (configuration.TryGetValue("instance.id", out var instanceId) && !string.IsNullOrWhiteSpace(instanceId))
            options.InstanceId = instanceId;

        if (configuration.TryGetValue("api.key", out var apiKey) && !string.IsNullOrWhiteSpace(apiKey))
            options.ApiKey = apiKey;

        if (configuration.TryGetValue("service.url", out var serviceUrl) && !string.IsNullOrWhiteSpace(serviceUrl))
            options.ServiceUrl = serviceUrl;

        return options;
    }
}