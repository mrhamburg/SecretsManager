using SecretsManager.Builder;

namespace SecretsManager.Vault;

public sealed class VaultSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "vault";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new VaultSecretProvider(options);
    }

    internal static VaultOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new VaultOptions();

        if (configuration.TryGetValue("url", out var url) && !string.IsNullOrWhiteSpace(url))
            options.Url = url;

        if (configuration.TryGetValue("token", out var token) && !string.IsNullOrWhiteSpace(token))
            options.Token = token;

        if (configuration.TryGetValue("mount.path", out var mountPath) && !string.IsNullOrWhiteSpace(mountPath))
            options.MountPath = mountPath;

        if (configuration.TryGetValue("skip.tls.verify", out var skipVerify) && !string.IsNullOrWhiteSpace(skipVerify))
            options.SkipTlsVerify = bool.TryParse(skipVerify, out var parsed) && parsed;

        return options;
    }
}
