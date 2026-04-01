using SecretsManager.Builder;

namespace SecretsManager.PostgreSql;

public sealed class PostgreSqlSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "postgresql";

    public ISecretProvider Create(IReadOnlyDictionary<string, string> configuration)
    {
        var options = MapConfiguration(configuration);
        return new PostgreSqlSecretProvider(options);
    }

    internal static PostgreSqlOptions MapConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        var options = new PostgreSqlOptions();

        if (configuration.TryGetValue("connection.string", out var cs) && !string.IsNullOrWhiteSpace(cs))
            options.ConnectionString = cs;

        if (configuration.TryGetValue("schema", out var schema) && !string.IsNullOrWhiteSpace(schema))
            options.Schema = schema;

        if (configuration.TryGetValue("table.prefix", out var prefix) && !string.IsNullOrWhiteSpace(prefix))
            options.TablePrefix = prefix;

        if (configuration.TryGetValue("auto.create.schema", out var autoCreate) && !string.IsNullOrWhiteSpace(autoCreate))
            options.AutoCreateSchema = bool.Parse(autoCreate);

        if (configuration.TryGetValue("encryption.key", out var encKey) && !string.IsNullOrWhiteSpace(encKey))
            options.EncryptionKey = encKey;

        if (configuration.TryGetValue("encryption.keyfile", out var encKeyFile) && !string.IsNullOrWhiteSpace(encKeyFile))
            options.EncryptionKeyFile = encKeyFile;

        return options;
    }
}
