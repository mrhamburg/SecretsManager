using SecretsManager.Builder;

namespace SecretsManager.Scaleway;

public sealed class ScalewaySecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "scaleway";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new ScalewaySecretProvider(options);
    }

    internal static ScalewayOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new ScalewayOptions();

        if (configuration.TryGetValue("region", out var region) && !string.IsNullOrWhiteSpace(region))
            options.Region = region;

        if (configuration.TryGetValue("project.id", out var projectId) && !string.IsNullOrWhiteSpace(projectId))
            options.ProjectId = projectId;

        if (configuration.TryGetValue("access.key", out var accessKey) && !string.IsNullOrWhiteSpace(accessKey))
            options.AccessKey = accessKey;

        if (configuration.TryGetValue("secret.key", out var secretKey) && !string.IsNullOrWhiteSpace(secretKey))
            options.SecretKey = secretKey;

        return options;
    }
}
