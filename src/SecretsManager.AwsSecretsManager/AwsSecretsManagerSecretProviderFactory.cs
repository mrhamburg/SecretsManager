using SecretsManager.Builder;

namespace SecretsManager.AwsSecretsManager;

public sealed class AwsSecretsManagerSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "awssecretsmanager";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new AwsSecretsManagerSecretProvider(options);
    }

    internal static AwsSecretsManagerOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new AwsSecretsManagerOptions();

        if (configuration.TryGetValue("region", out var region) && !string.IsNullOrWhiteSpace(region))
            options.Region = region;

        if (configuration.TryGetValue("access.key", out var accessKey) && !string.IsNullOrWhiteSpace(accessKey))
            options.AccessKey = accessKey;

        if (configuration.TryGetValue("secret.key", out var secretKey) && !string.IsNullOrWhiteSpace(secretKey))
            options.SecretKey = secretKey;

        if (configuration.TryGetValue("session.token", out var sessionToken) && !string.IsNullOrWhiteSpace(sessionToken))
            options.SessionToken = sessionToken;

        return options;
    }
}
