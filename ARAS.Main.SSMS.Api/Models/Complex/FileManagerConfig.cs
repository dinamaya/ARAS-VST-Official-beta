namespace ARAS.Main.SSMS.Api.Models.Complex
{
	public class FileManagerConfig
	{
		public int MaxSize { get; set; }
		public IEnumerable<string> AllowedFileTypes { get; set; }
	}
}
