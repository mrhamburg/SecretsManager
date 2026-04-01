---
id: json-extraction
title: JSON Property Extraction
sidebar_label: JSON Extraction
sidebar_position: 2
---

# JSON Property Extraction

SecretsManager can extract nested values from JSON-formatted secrets using dot-path syntax. This is useful when a single secret contains multiple related values.

## Basic Usage

If a secret stores a JSON object, you can extract specific properties:

```csharp
// Secret "db-config" contains:
// {"host": "db.example.com", "port": 5432, "username": "admin", "password": "s3cret"}

var query = new SecretQuery { Property = "password" };
var password = await provider.GetSecretAsync("db-config", query);
Console.WriteLine(password.Value); // "s3cret"
```

## Nested Properties

Use dot notation to access nested values:

```csharp
// Secret "app-config" contains:
// {"database": {"connection": {"host": "db.example.com", "port": 5432}}}

var query = new SecretQuery { Property = "database.connection.host" };
var host = await provider.GetSecretAsync("app-config", query);
Console.WriteLine(host.Value); // "db.example.com"
```

## Combining with Versioning

You can combine property extraction with version queries:

```csharp
var query = new SecretQuery
{
    Version = "v2",
    Property = "database.password"
};

var secret = await provider.GetSecretAsync("config", query);
```

:::info
JSON extraction is handled by the core library's `JsonPropertyExtractor` and works identically across all providers. The extraction happens after the secret value is retrieved from the backend.
:::
