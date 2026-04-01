namespace SecretsManager.IBMCloudSecretsManager;

public sealed class IBMCloudSecretsManagerOptions
{
    /// <summary>
    /// IBM Cloud region (e.g. "us-south", "eu-de", "jp-tok").
    /// </summary>
    public string Region { get; set; } = "";

    /// <summary>
    /// IBM Cloud instance ID for the Secrets Manager instance.
    /// </summary>
    public string InstanceId { get; set; } = "";

    /// <summary>
    /// IBM Cloud API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// IBM Cloud service URL override (optional).
    /// </summary>
    public string? ServiceUrl { get; set; }
}