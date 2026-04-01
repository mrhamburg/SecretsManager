using SecretsManager.Builder;

namespace SecretsManager.PostgreSql;

public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithPostgreSql(
        this SecretProviderBuilder builder,
        Action<PostgreSqlOptions>? configure = null)
    {
        builder.RegisterProvider(new PostgreSqlSecretProviderFactory());

        if (configure is null) return builder;

        var options = new PostgreSqlOptions();
        configure(options);

        builder.UseProvider("postgresql", settings =>
        {
            settings["connection.string"] = options.ConnectionString;
            settings["schema"] = options.Schema;
            settings["table.prefix"] = options.TablePrefix;
            settings["auto.create.schema"] = options.AutoCreateSchema.ToString();

            if (!string.IsNullOrEmpty(options.EncryptionKey))
                settings["encryption.key"] = options.EncryptionKey;

            if (!string.IsNullOrEmpty(options.EncryptionKeyFile))
                settings["encryption.keyfile"] = options.EncryptionKeyFile;
        });

        return builder;
    }
}
