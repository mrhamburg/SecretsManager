using System.Text.Json;

namespace SecretsManager.Internal;

/// <summary>
/// Extracts a dot-separated property path from a JSON string.
/// </summary>
public static class JsonPropertyExtractor
{
    public static string Extract(string json, string propertyPath)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new SecretProviderException(
                $"Cannot extract property '{propertyPath}': secret value is not valid JSON.", ex);
        }

        using (doc)
        {
            var segments = propertyPath.Split('.');
            var current = doc.RootElement;

            foreach (var segment in segments)
            {
                if (current.ValueKind != JsonValueKind.Object)
                    throw new SecretProviderException(
                        $"Cannot extract property '{propertyPath}': " +
                        $"segment '{segment}' is not an object (found {current.ValueKind}).");

                if (!current.TryGetProperty(segment, out var next))
                    throw new SecretProviderException(
                        $"Property '{propertyPath}' not found in secret value " +
                        $"(missing segment '{segment}').");

                current = next;
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString()!,
                JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False =>
                    current.GetRawText(),
                _ => current.GetRawText() // objects, arrays → raw JSON
            };
        }
    }
}
