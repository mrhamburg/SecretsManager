---
id: versioning
title: Secret Versioning
sidebar_label: Versioning
sidebar_position: 1
---

# Secret Versioning

All SecretsManager providers support secret versioning, allowing you to maintain a complete history of secret values.

## How Versioning Works

Every call to `PutSecretAsync` creates a new version of the secret. The latest version is always returned by default when calling `GetSecretAsync`.

```csharp
// Create initial version
await provider.PutSecretAsync("db-password", "initial-password");

// Update creates a new version
await provider.PutSecretAsync("db-password", "updated-password");

// Gets the latest version by default
var current = await provider.GetSecretAsync("db-password");
Console.WriteLine(current.Value); // "updated-password"
```

## Listing Versions

Use `GetSecretVersionsAsync` to retrieve the full version history:

```csharp
var versions = await provider.GetSecretVersionsAsync("db-password");

foreach (var version in versions)
{
    Console.WriteLine($"Version: {version.Version}");
    Console.WriteLine($"Created: {version.CreatedAt}");
}
```

## Retrieving a Specific Version

Pass a `SecretQuery` with the version identifier to retrieve a historical value:

```csharp
var query = new SecretQuery { Version = "v1" };
var oldSecret = await provider.GetSecretAsync("db-password", query);
Console.WriteLine(oldSecret.Value); // "initial-password"
```

:::tip
Version identifiers vary by provider. FileSystem uses `v1`, `v2`, etc. Azure Key Vault uses Azure-generated version IDs. Scaleway uses revision numbers. The `GetSecretVersionsAsync` method returns the correct identifiers for each provider.
:::

## Provider-Specific Behavior

| Provider | Version Format | Auto-Versioning | Max Versions |
|----------|:-------------:|:---------------:|:------------:|
| FileSystem | v1, v2, v3... | Yes | Unlimited |
| Azure Key Vault | Azure GUID | Yes | Unlimited |
| Scaleway | Revision number | Yes | Unlimited |
