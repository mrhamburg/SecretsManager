namespace SecretsManager.TencentCloud;

public sealed class TencentCloudOptions
{
    public string Region { get; set; } = "";

    public string SecretId { get; set; } = "";

    public string SecretKey { get; set; } = "";

    public string? Endpoint { get; set; }
}
