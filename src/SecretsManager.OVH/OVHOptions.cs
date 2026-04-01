namespace SecretsManager.OVH;

public sealed class OVHOptions
{
    /// <summary>
    /// OVH endpoint region (e.g. "ovh-eu", "ovh-us", "ovh-ca").
    /// </summary>
    public string Endpoint { get; set; } = "";

    /// <summary>
    /// OVH application key.
    /// </summary>
    public string ApplicationKey { get; set; } = "";

    /// <summary>
    /// OVH application secret.
    /// </summary>
    public string ApplicationSecret { get; set; } = "";

    /// <summary>
    /// OVH consumer key.
    /// </summary>
    public string ConsumerKey { get; set; } = "";

    /// <summary>
    /// OVH OKMS ID.
    /// </summary>
    public string OkmsId { get; set; } = "";
}