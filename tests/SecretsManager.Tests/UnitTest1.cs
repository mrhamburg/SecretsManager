namespace SecretsManager.Tests;

public class SecretValueTests
{
    [Fact]
    public void SecretValue_RequiredProperties_AreSet()
    {
        var secret = new SecretValue { Key = "db-password", Value = "s3cret" };

        Assert.Equal("db-password", secret.Key);
        Assert.Equal("s3cret", secret.Value);
    }

    [Fact]
    public void SecretValue_OptionalProperties_DefaultToNull()
    {
        var secret = new SecretValue { Key = "key", Value = "val" };

        Assert.Null(secret.Version);
        Assert.Null(secret.CreatedAt);
        Assert.Null(secret.UpdatedAt);
        Assert.Null(secret.ContentType);
        Assert.Null(secret.Tags);
    }

    [Fact]
    public void SecretValue_WithAllProperties_RoundTrips()
    {
        var now = DateTimeOffset.UtcNow;
        var tags = new Dictionary<string, string> { ["env"] = "prod" };

        var secret = new SecretValue
        {
            Key = "api-key",
            Value = "abc123",
            Version = "v2",
            CreatedAt = now,
            UpdatedAt = now,
            ContentType = "text/plain",
            Tags = tags
        };

        Assert.Equal("api-key", secret.Key);
        Assert.Equal("abc123", secret.Value);
        Assert.Equal("v2", secret.Version);
        Assert.Equal(now, secret.CreatedAt);
        Assert.Equal("text/plain", secret.ContentType);
        Assert.Equal("prod", secret.Tags!["env"]);
    }

    [Fact]
    public void SecretValue_RecordEquality_WorksByValue()
    {
        var a = new SecretValue { Key = "k", Value = "v", Version = "1" };
        var b = new SecretValue { Key = "k", Value = "v", Version = "1" };

        Assert.Equal(a, b);
    }

    [Fact]
    public void SecretValue_With_CreatesModifiedCopy()
    {
        var original = new SecretValue { Key = "k", Value = "v1" };
        var updated = original with { Value = "v2" };

        Assert.Equal("v1", original.Value);
        Assert.Equal("v2", updated.Value);
        Assert.Equal("k", updated.Key);
    }
}

public class SecretQueryTests
{
    [Fact]
    public void SecretQuery_Defaults_AreNull()
    {
        var query = new SecretQuery();

        Assert.Null(query.Version);
        Assert.Null(query.Property);
    }

    [Fact]
    public void SecretQuery_WithVersion_SetsVersion()
    {
        var query = new SecretQuery { Version = "v3" };

        Assert.Equal("v3", query.Version);
    }

    [Fact]
    public void SecretQuery_WithProperty_SetsProperty()
    {
        var query = new SecretQuery { Property = "connection.host" };

        Assert.Equal("connection.host", query.Property);
    }
}

public class SecretMetadataTests
{
    [Fact]
    public void SecretMetadata_Defaults_AreNull()
    {
        var metadata = new SecretMetadata();

        Assert.Null(metadata.ContentType);
        Assert.Null(metadata.Tags);
    }

    [Fact]
    public void SecretMetadata_WithTags_StoresTags()
    {
        var tags = new Dictionary<string, string> { ["team"] = "platform" };
        var metadata = new SecretMetadata { ContentType = "application/json", Tags = tags };

        Assert.Equal("application/json", metadata.ContentType);
        Assert.Equal("platform", metadata.Tags!["team"]);
    }
}

public class SecretVersionInfoTests
{
    [Fact]
    public void SecretVersionInfo_RequiredVersion_IsSet()
    {
        var info = new SecretVersionInfo { Version = "v1" };

        Assert.Equal("v1", info.Version);
        Assert.False(info.IsCurrent);
        Assert.Null(info.CreatedAt);
    }

    [Fact]
    public void SecretVersionInfo_CurrentVersion_IsMarked()
    {
        var info = new SecretVersionInfo { Version = "v3", IsCurrent = true };

        Assert.True(info.IsCurrent);
    }
}

public class SecretExceptionTests
{
    [Fact]
    public void SecretNotFoundException_ContainsKey()
    {
        var ex = new SecretNotFoundException("my-secret");

        Assert.Equal("my-secret", ex.Key);
        Assert.Contains("my-secret", ex.Message);
    }

    [Fact]
    public void SecretNotFoundException_IsSecretProviderException()
    {
        var ex = new SecretNotFoundException("key");

        Assert.IsAssignableFrom<SecretProviderException>(ex);
    }

    [Fact]
    public void SecretNotFoundException_WithInnerException_Wraps()
    {
        var inner = new InvalidOperationException("gone");
        var ex = new SecretNotFoundException("key", inner);

        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void SecretProviderException_IsException()
    {
        var ex = new SecretProviderException("backend unavailable");

        Assert.IsAssignableFrom<Exception>(ex);
        Assert.Equal("backend unavailable", ex.Message);
    }
}
