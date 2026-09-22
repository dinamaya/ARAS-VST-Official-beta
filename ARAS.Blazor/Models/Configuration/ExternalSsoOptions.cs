namespace ARAS.Blazor.Models.Configuration;

public sealed class ExternalSsoOptions
{
	public const string SectionName = "ExternalSso";

	public string AccessKey { get; set; } = string.Empty;

	public string ReturnUrl { get; set; } = string.Empty;

	public string? ApplicationCode { get; set; }

	public string? ApplicationSubApplicationCode { get; set; }

	public HashSet<string> AllowedRoles { get; set; } =
		new(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, string> RoleMappings { get; set; } =
		new(StringComparer.OrdinalIgnoreCase);
}
