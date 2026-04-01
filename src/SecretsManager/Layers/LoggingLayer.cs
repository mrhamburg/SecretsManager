using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SecretsManager;

/// <summary>
/// Adds logging hooks around provider operations.
/// </summary>
public sealed class LoggingLayer : ISecretProviderLayer
{
    private readonly LoggingLayerOptions _options;

    public LoggingLayer(LoggingLayerOptions? options = null)
    {
        _options = options ?? new LoggingLayerOptions();
        if (_options.Logger is null)
            throw new ArgumentNullException(nameof(LoggingLayerOptions.Logger));
    }

    public ISecretProvider Wrap(ISecretProvider inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        return new LoggingSecretProvider(inner, _options);
    }

    private sealed class LoggingSecretProvider(ISecretProvider inner, LoggingLayerOptions options) : ISecretProvider
    {
        private readonly ISecretProvider _inner = inner;
        private readonly ISecretProviderLogger _logger = options.Logger;
        private readonly string _operationPrefix = string.IsNullOrWhiteSpace(options.OperationPrefix)
            ? string.Empty
            : options.OperationPrefix + ".";

        public async Task<SecretValue> GetSecretAsync(
            string key,
            SecretQuery? query = null,
            CancellationToken cancellationToken = default)
        {
            var context = new SecretProviderLogContext
            {
                Operation = ComposeOperation("GetSecretAsync"),
                Key = key,
                IsMetadataWrite = false
            };

            return await ExecuteAsync(
                () => _inner.GetSecretAsync(key, query, cancellationToken),
                context);
        }

        public async Task<SecretValue> PutSecretAsync(
            string key,
            string value,
            SecretMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            var context = new SecretProviderLogContext
            {
                Operation = ComposeOperation("PutSecretAsync"),
                Key = key,
                IsMetadataWrite = metadata is not null
            };

            return await ExecuteAsync(
                () => _inner.PutSecretAsync(key, value, metadata, cancellationToken),
                context);
        }

        public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            var context = new SecretProviderLogContext
            {
                Operation = ComposeOperation("DeleteSecretAsync"),
                Key = key,
                IsMetadataWrite = false
            };

            await ExecuteAsync(
                () => _inner.DeleteSecretAsync(key, cancellationToken),
                context);
        }

        public async Task<IReadOnlyList<SecretVersionInfo>> GetSecretVersionsAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            var context = new SecretProviderLogContext
            {
                Operation = ComposeOperation("GetSecretVersionsAsync"),
                Key = key,
                IsMetadataWrite = false
            };

            return await ExecuteAsync(
                () => _inner.GetSecretVersionsAsync(key, cancellationToken),
                context);
        }

        public async Task<bool> SecretExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            var context = new SecretProviderLogContext
            {
                Operation = ComposeOperation("SecretExistsAsync"),
                Key = key,
                IsMetadataWrite = false
            };

            return await ExecuteAsync(
                () => _inner.SecretExistsAsync(key, cancellationToken),
                context);
        }

        public ValueTask DisposeAsync() => _inner.DisposeAsync();

        private async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, SecretProviderLogContext context)
        {
            _logger.LogStart(context);

            var started = Stopwatch.GetTimestamp();
            try
            {
                var result = await operation().ConfigureAwait(false);
                _logger.LogSuccess(context, GetElapsed(started));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogFailure(context, ex, GetElapsed(started));
                throw;
            }
        }

        private async Task ExecuteAsync(Func<Task> operation, SecretProviderLogContext context)
        {
            _logger.LogStart(context);

            var started = Stopwatch.GetTimestamp();
            try
            {
                await operation().ConfigureAwait(false);
                _logger.LogSuccess(context, GetElapsed(started));
            }
            catch (Exception ex)
            {
                _logger.LogFailure(context, ex, GetElapsed(started));
                throw;
            }
        }

        private TimeSpan GetElapsed(long startTimestamp)
        {
            var end = Stopwatch.GetTimestamp();
            return TimeSpan.FromSeconds((double)(end - startTimestamp) / Stopwatch.Frequency);
        }

        private string ComposeOperation(string operation)
        {
            return _operationPrefix + operation;
        }
    }
}
