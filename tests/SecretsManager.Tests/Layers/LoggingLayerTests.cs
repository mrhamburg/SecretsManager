using System;
using System.Threading.Tasks;

namespace SecretsManager.Tests;

public sealed class LoggingLayerTests
{
    [Fact]
    public async Task LoggingLayer_ForwardsLifecycleEvents()
    {
        var logger = new RecordingLogger();

        var baseProvider = new DelegatingSecretProvider
        {
            GetSecretAsyncImpl = (key, _, _) =>
            {
                if (key == "explode")
                    return Task.FromException<SecretValue>(new InvalidOperationException("boom"));

                return Task.FromResult(new SecretValue { Key = "key", Value = "ok" });
            },
            PutSecretAsyncImpl = (key, value, _, _) => Task.FromResult(new SecretValue { Key = key, Value = value })
        };

        await using var provider = new LoggingLayer(new LoggingLayerOptions
        {
            Logger = logger,
            OperationPrefix = "Test"
        }).Wrap(baseProvider);

        await provider.GetSecretAsync("key");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GetSecretAsync("explode"));

        Assert.Equal(2, logger.Entries.Count(e => e.Type == "start"));
        Assert.Contains(logger.Entries, e => e.Type == "success" && e.Operation == "Test.GetSecretAsync");
        Assert.Contains(logger.Entries, e => e.Type == "failure" && e.Exception is InvalidOperationException);
    }
}
