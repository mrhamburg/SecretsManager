namespace SecretsManager.Builder;

/// <summary>
/// Intermediate configuration model holding the selected provider name
/// and its flat key-value settings.
/// </summary>
public sealed class SecretProviderConfiguration
{
    public string ProviderName { get; set; } = "";
    public Dictionary<string, string> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
