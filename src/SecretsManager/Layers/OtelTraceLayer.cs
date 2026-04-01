using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SecretsManager;

/// <summary>
/// Adds OpenTelemetry-compatible activity spans for provider operations.
/// </summary>
public sealed class OtelTraceLayer : ISecretProviderLayer
{
    private readonly OtelTraceLayerOptions _options;

    public OtelTraceLayer(OtelTraceLayerOptions? options = null)
    {
        _options = options ?? new OtelTraceLayerOptions();

        if (string.IsNullOrWhiteSpace(_options.ActivitySourceName))
            throw new ArgumentException("ActivitySourceName is required.", nameof(OtelTraceLayerOptions.ActivitySourceName));
    }

    public ISecretProvider Wrap(ISecretProvider inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        return new OtelTraceSecretProvider(inner, _options);
    }

    private sealed class OtelTraceSecretProvider(ISecretProvider inner, OtelTraceLayerOptions options) : ISecretProvider
    {
        private readonly ISecretProvider _inner = inner;
        private readonly ActivitySource _activitySource = new(options.ActivitySourceName, options.ActivitySourceVersion);
        private readonly bool _recordException = options.RecordException;

        public async Task<SecretValue> GetSecretAsync(
            string key,
            SecretQuery? query = null,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                "GetSecretAsync",
                key,
                token => _inner.GetSecretAsync(key, query, token),
                cancellationToken);
        }

        public async Task<SecretValue> PutSecretAsync(
            string key,
            string value,
            SecretMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                "PutSecretAsync",
                key,
                token => _inner.PutSecretAsync(key, value, metadata, token),
                cancellationToken);
        }

        public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(
                "DeleteSecretAsync",
                key,
                token => _inner.DeleteSecretAsync(key, token),
                cancellationToken);
        }

        public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                "GetSecretVersionsAsync",
                key,
                token => _inner.GetSecretVersionsAsync(key, token),
                cancellationToken);
        }

        public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                "SecretExistsAsync",
                key,
                token => _inner.SecretExistsAsync(key, token),
                cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            _activitySource.Dispose();
            return _inner.DisposeAsync();
        }

        private async Task<TResult> ExecuteAsync<TResult>(
            string operation,
            string key,
            Func<CancellationToken, Task<TResult>> operationExecutor,
            CancellationToken cancellationToken)
        {
            using var activity = _activitySource.StartActivity(operation, ActivityKind.Client);
            activity?.SetTag("secrets.operation", operation);
            if (!string.IsNullOrWhiteSpace(key))
                activity?.SetTag("secrets.key", key);

            try
            {
                var result = await operationExecutor(cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Ok);
                return result;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                if (_recordException)
                {
                    activity?.AddTag("exception.type", ex.GetType().FullName);
                    activity?.AddTag("exception.message", ex.Message);
                    activity?.AddTag("exception.stacktrace", ex.StackTrace);
                }

                throw;
            }
        }

        private async Task ExecuteAsync(
            string operation,
            string key,
            Func<CancellationToken, Task> operationExecutor,
            CancellationToken cancellationToken)
        {
            using var activity = _activitySource.StartActivity(operation, ActivityKind.Client);
            activity?.SetTag("secrets.operation", operation);
            if (!string.IsNullOrWhiteSpace(key))
                activity?.SetTag("secrets.key", key);

            try
            {
                await operationExecutor(cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                if (_recordException)
                {
                    activity?.AddTag("exception.type", ex.GetType().FullName);
                    activity?.AddTag("exception.message", ex.Message);
                    activity?.AddTag("exception.stacktrace", ex.StackTrace);
                }

                throw;
            }
        }
    }
}
