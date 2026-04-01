namespace SecretsManager.AzureKeyVault;

public sealed class AzureKeyVaultOptions
{
    /// <summary>
    /// The Azure Key Vault URI (e.g. "https://myvault.vault.azure.net").
    /// </summary>
    public string VaultUrl { get; set; } = "";

    /// <summary>
    /// Authentication type: "default", "serviceprincipal", "managedidentity", "workloadidentity".
    /// Defaults to "default" (DefaultAzureCredential).
    /// </summary>
    public string AuthenticationType { get; set; } = "default";

    /// <summary>
    /// Azure AD tenant ID. Required for service principal authentication.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Azure AD application (client) ID.
    /// Required for service principal; optional for managed identity (user-assigned).
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Azure AD client secret. Required for service principal authentication.
    /// </summary>
    public string? ClientSecret { get; set; }
}
