using System.Collections.Generic;
using System.Threading.Tasks;
using SecretsManager.Builder;

namespace SecretsManager.Tests;

public sealed class LayerPipelineTests
{
    [Fact]
    public async Task Build_AppliesLayersInRegistrationOrder()
    {
        var callOrder = new List<string>();
        var provider = new DelegatingSecretProvider
        {
            GetSecretAsyncImpl = (key, _, _) =>
            {
                callOrder.Add("base-get");
                return Task.FromResult(new SecretValue { Key = key, Value = "value" });
            },
            PutSecretAsyncImpl = (_, value, _, _) =>
            {
                callOrder.Add("base-put");
                return Task.FromResult(new SecretValue { Key = "", Value = value });
            }
        };

        var factory = new TestLayerProviderFactory(provider);
        var builder = new SecretProviderBuilder().RegisterProvider(factory).UseProvider("test");

        builder.AddLayer(new TestTrackingLayer("first", callOrder));
        builder.AddLayer(new TestTrackingLayer("second", callOrder));

        await using var built = builder.Build();

        await built.GetSecretAsync("my-key");

        Assert.Equal(
            new[]
            {
                "first-enter-get",
                "second-enter-get",
                "base-get",
                "second-exit-get",
                "first-exit-get"
            },
            callOrder);
    }
}
