using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SecretsManager.Tests;

public sealed class OtelTraceLayerTests
{
    [Fact]
    public async Task OtelTraceLayer_EmitsActivity()
    {
        const string sourceName = "SecretsManager.TestTrace";
        var activities = new List<Activity>();

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activities.Add
        };
        ActivitySource.AddActivityListener(listener);

        var provider = new DelegatingSecretProvider
        {
            GetSecretAsyncImpl = (_, _, _) => Task.FromResult(new SecretValue { Key = "key", Value = "value" })
        };

        await using var traced = new OtelTraceLayer(new OtelTraceLayerOptions
        {
            ActivitySourceName = sourceName
        }).Wrap(provider);

        await traced.GetSecretAsync("key");

        Assert.Single(activities);
        Assert.Equal("GetSecretAsync", activities[0].DisplayName);
        Assert.Equal("GetSecretAsync", activities[0].GetTagItem("secrets.operation")?.ToString());
    }

    [Fact]
    public async Task OtelTraceLayer_RecordsExceptionsAsTags()
    {
        const string sourceName = "SecretsManager.TraceFailureTest";
        var activities = new List<Activity>();

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activities.Add
        };
        ActivitySource.AddActivityListener(listener);

        var provider = new DelegatingSecretProvider
        {
            GetSecretAsyncImpl = (_, _, _) => Task.FromException<SecretValue>(new InvalidOperationException("boom"))
        };

        await using var traced = new OtelTraceLayer(new OtelTraceLayerOptions
        {
            ActivitySourceName = sourceName
        }).Wrap(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(() => traced.GetSecretAsync("bad"));

        Assert.Single(activities);
        Assert.Equal(ActivityStatusCode.Error, activities[0].Status);
        Assert.Equal("System.InvalidOperationException", activities[0].GetTagItem("exception.type")?.ToString());
    }
}
