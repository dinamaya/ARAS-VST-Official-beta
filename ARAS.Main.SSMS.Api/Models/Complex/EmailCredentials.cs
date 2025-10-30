namespace ARAS.Main.SSMS.Api.Models.Complex
{
	public class EmailCredentials
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string EmailAddress { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string ReplyAddress { get; set; }
		public bool EnableSsl { get; set; }
	}
}
