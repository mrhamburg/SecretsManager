---
id: custom-providers
title: Building Custom Providers
sidebar_label: Custom Providers
sidebar_position: 3
---

# Building Custom Providers

SecretsManager is designed to be extensible. You can implement your own provider for any secret backend.

## Implement ISecretProvider

Create a class that implements the `ISecretProvider` interface:

```csharp title="MyCustomSecretProvider.cs"
using SecretsManager;

public class MyCustomSecretProvider : ISecretProvider
{
    public async Task<SecretValue> GetSecretAsync(
        string key, SecretQuery? query = null, CancellationToken ct = default)
    {
        // Retrieve secret from your backend
        var value = await YourBackend.GetAsync(key, ct);
        return new SecretValue(key, value);
    }

    public async Task<SecretValue> PutSecretAsync(
        string key, string value, SecretMetadata? metadata = null, CancellationToken ct = default)
    {
        await YourBackend.SetAsync(key, value, ct);
        return new SecretValue(key, value);
    }

    public async Task DeleteSecretAsync(string key, CancellationToken ct = default)
    {
        await YourBackend.DeleteAsync(key, ct);
    }

    public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
        string key, CancellationToken ct = default)
    {
        return await YourBackend.ListVersionsAsync(key, ct);
    }

    public async Task<bool> SecretExistsAsync(string key, CancellationToken ct = default)
    {
        return await YourBackend.ExistsAsync(key, ct);
    }

    public ValueTask DisposeAsync()
    {
        // Clean up resources
        return ValueTask.CompletedTask;
    }
}
```

## Implement ISecretProviderFactory

To integrate with the builder pattern, implement `ISecretProviderFactory`:

```csharp title="MyCustomSecretProviderFactory.cs"
using SecretsManager.Builder;

public class MyCustomSecretProviderFactory : ISecretProviderFactory
{
    public string ProviderName => "mycustom";

    public ISecretProvider Create(Dictionary<string, string> settings)
    {
        var endpoint = settings["endpoint"];
        var apiKey = settings["apiKey"];
        return new MyCustomSecretProvider(endpoint, apiKey);
    }
}
```

## Add Builder Extension

Create a fluent extension method:

```csharp title="SecretProviderBuilderExtensions.cs"
public static class SecretProviderBuilderExtensions
{
    public static SecretProviderBuilder WithMyCustom(
        this SecretProviderBuilder builder,
        Action<MyCustomOptions>? configure = null)
    {
        builder.RegisterFactory(new MyCustomSecretProviderFactory());
        if (configure != null)
        {
            var options = new MyCustomOptions();
            configure(options);
            builder.ApplyOptions(options);
        }
        return builder;
    }
}
```

:::tip
Follow the existing provider implementations in the repository as reference. The FileSystem provider is the simplest starting point.
:::
