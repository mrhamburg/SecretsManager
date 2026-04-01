using SecretsManager.Builder;

namespace SecretsManager.FileSystem;

/// <summary>
/// Extension methods to register the filesystem provider with <see cref="SecretProviderBuilder"/>.
/// </summary>
public static class SecretProviderBuilderExtensions
{
    /// <summary>
    /// Registers the filesystem provider and optionally configures it via the fluent API.
    /// When combined with <see cref="SecretProviderBuilder.FromEnvironment"/> or
    /// <see cref="SecretProviderBuilder.FromYaml"/>, the factory is registered but
    /// configuration comes from the external source.
    /// </summary>
    public static SecretProviderBuilder WithFileSystem(
        this SecretProviderBuilder builder,
        Action<FileSystemOptions>? configure = null)
    {
        builder.RegisterProvider(new FileSystemSecretProviderFactory());

        if (configure is not null)
        {
            var options = new FileSystemOptions();
            configure(options);

            builder.UseProvider("filesystem", settings =>
            {
                settings["path"] = options.BasePath;
                if (options.EncryptionKey is not null)
                    settings["encryption.key"] = options.EncryptionKey;
                if (options.EncryptionKeyFile is not null)
                    settings["encryption.keyfile"] = options.EncryptionKeyFile;
            });
        }

        return builder;
    }
}
