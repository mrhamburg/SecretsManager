using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace SecretsManager.FileSystem.Internal;

/// <summary>
/// Handles all file I/O for the filesystem secret provider.
/// Provides per-key locking for thread safety and atomic writes via temp-file-then-rename.
/// </summary>
internal sealed class SecretFileStore : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly char[] InvalidKeyChars =
        ['/', '\\', ':', '*', '?', '"', '<', '>', '|', '\0'];

    private readonly string _basePath;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);

    public SecretFileStore(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(basePath);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            File.SetUnixFileMode(basePath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    public async Task<VersionedSecretEnvelope> ReadEnvelopeAsync(string key, string version, CancellationToken ct)
    {
        var path = GetVersionPath(key, version);
        if (!File.Exists(path))
            throw new SecretNotFoundException(key);

        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<VersionedSecretEnvelope>(json, JsonOptions)
            ?? throw new SecretProviderException($"Corrupt envelope for secret '{key}' version '{version}'.");
    }

    public async Task WriteEnvelopeAsync(string key, VersionedSecretEnvelope envelope, CancellationToken ct)
    {
        var dir = GetSecretDirectory(key);
        Directory.CreateDirectory(dir);

        var targetPath = GetVersionPath(key, envelope.Version);
        var tempPath = targetPath + ".tmp";

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        await File.WriteAllTextAsync(tempPath, json, ct);
        File.Move(tempPath, targetPath, overwrite: true);
    }

    public async Task<string> ReadCurrentVersionAsync(string key, CancellationToken ct)
    {
        var path = GetCurrentPointerPath(key);
        if (!File.Exists(path))
            throw new SecretNotFoundException(key);

        var json = await File.ReadAllTextAsync(path, ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("version").GetString()
            ?? throw new SecretProviderException($"Invalid current pointer for secret '{key}'.");
    }

    public async Task WriteCurrentVersionAsync(string key, string version, CancellationToken ct)
    {
        var dir = GetSecretDirectory(key);
        Directory.CreateDirectory(dir);

        var targetPath = GetCurrentPointerPath(key);
        var tempPath = targetPath + ".tmp";

        var json = JsonSerializer.Serialize(new { version }, JsonOptions);
        await File.WriteAllTextAsync(tempPath, json, ct);
        File.Move(tempPath, targetPath, overwrite: true);
    }

    public string GetNextVersion(string key)
    {
        var dir = GetSecretDirectory(key);
        if (!Directory.Exists(dir))
            return "v1";

        var maxVersion = Directory.GetFiles(dir, "v*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(f => f != "current")
            .Select(f => int.TryParse(f!.AsSpan(1), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"v{maxVersion + 1}";
    }

    public List<string> GetVersions(string key)
    {
        var dir = GetSecretDirectory(key);
        if (!Directory.Exists(dir))
            return [];

        return Directory.GetFiles(dir, "v*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(f => f != "current")
            .Where(f => f is not null && f.Length > 1 && int.TryParse(f.AsSpan(1), out _))
            .OrderBy(f => int.Parse(f!.AsSpan(1)))
            .ToList()!;
    }

    public bool SecretExists(string key)
    {
        return File.Exists(GetCurrentPointerPath(key));
    }

    public void DeleteSecret(string key)
    {
        var dir = GetSecretDirectory(key);
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }

    public SemaphoreSlim GetLock(string key) =>
        _locks.GetOrAdd(SanitizeKey(key), _ => new SemaphoreSlim(1, 1));

    public void Dispose()
    {
        foreach (var sem in _locks.Values)
            sem.Dispose();
        _locks.Clear();
    }

    private string GetSecretDirectory(string key) =>
        Path.Combine(_basePath, SanitizeKey(key));

    private string GetVersionPath(string key, string version) =>
        Path.Combine(GetSecretDirectory(key), $"{version}.json");

    private string GetCurrentPointerPath(string key) =>
        Path.Combine(GetSecretDirectory(key), "current.json");

    internal static string SanitizeKey(string key)
    {
        var sanitized = key;
        foreach (var c in InvalidKeyChars)
            sanitized = sanitized.Replace(c, '_');
        return sanitized;
    }
}
