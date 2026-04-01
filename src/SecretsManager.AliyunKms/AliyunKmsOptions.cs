namespace SecretsManager.AliyunKms;

public sealed class AliyunKmsOptions
{
    public string Region { get; set; } = "";

    public string AccessKeyId { get; set; } = "";

    public string AccessKeySecret { get; set; } = "";

    public string? Endpoint { get; set; }
}
