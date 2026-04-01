using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace SecretsManager.Tests;

public sealed class OtelMetricsLayerTests
{
    [Fact]
    public async Task OtelMetricsLayer_DoesNotBreakOperationFlow()
    {
        var provider = new DelegatingSecretProvider
        {
            GetSecretAsyncImpl = (_, _, _) => Task.FromResult(new SecretValue { Key = "k", Value = "v" })
        };

        await using var layered = new OtelMetricsLayer(new OtelMetricsLayerOptions
        {
            MeterName = "secretsmanager.test.metrics",
            MetricPrefix = "sm."
        }).Wrap(provider);

        await layered.GetSecretAsync("k");

        Assert.NotNull(layered);
    }

    [Fact]
    public async Task OtelMetricsLayer_EmitsExpectedMeasurements()
    {
        const string meterName = "secretsmanager.metrics.tests";
        const string metricPrefix = "sm";

        var longMeasurements = new List<(string Name, long Value)>();
        var doubleMeasurements = new List<(string Name, double Value)>();

        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, registered) =>
            {
                if (instrument.Meter.Name == meterName)
                    registered.EnableMeasurementEvents(instrument);
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, value, _, _) =>
            longMeasurements.Add((instrument.Name, value)));
        listener.SetMeasurementEventCallback<double>((instrument, value, _, _) =>
            doubleMeasurements.Add((instrument.Name, value)));
        listener.Start();

        var provider = new DelegatingSecretProvider
        {
            GetSecretAsyncImpl = (key, _, _) =>
            {
                if (key == "fail")
                    return Task.FromException<SecretValue>(new InvalidOperationException("boom"));

                return Task.FromResult(new SecretValue { Key = key, Value = "ok" });
            }
        };

        await using var layered = new OtelMetricsLayer(new OtelMetricsLayerOptions
        {
            MeterName = meterName,
            MetricPrefix = metricPrefix
        }).Wrap(provider);

        await layered.GetSecretAsync("ok");
        await Assert.ThrowsAsync<InvalidOperationException>(() => layered.GetSecretAsync("fail"));

        Assert.Contains(longMeasurements, m => m.Name == "sm.operation.calls");
        Assert.Contains(longMeasurements, m => m.Name == "sm.operation.failures");
        Assert.Contains(doubleMeasurements, m => m.Name == "sm.operation.duration_ms");
    }
}
