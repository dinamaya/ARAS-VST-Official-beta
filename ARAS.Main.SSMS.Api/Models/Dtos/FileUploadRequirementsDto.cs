namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class FileUploadRequirementsDto
	{
		public int MaxSize { get; set; }
		public string AllowTypesMessage { get; set; }
		public IEnumerable<string> AllowedTypes { get; set; }
	}
}
