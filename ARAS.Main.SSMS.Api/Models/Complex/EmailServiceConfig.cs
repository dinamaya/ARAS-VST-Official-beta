namespace ARAS.Main.SSMS.Api.Models.Complex
{
	public class EmailServiceConfig
	{
		public EmailCredentials Credentials { get; set; }
		public IEnumerable<string> To { get; set; }
		public IEnumerable<string> Bcc { get; set; }
		public IEnumerable<string> Cc { get; set; }
	}
}
