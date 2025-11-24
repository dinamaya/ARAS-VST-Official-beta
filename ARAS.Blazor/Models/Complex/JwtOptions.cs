namespace ARAS.Blazor.Models.Complex
{
	public class JwtOptions
	{
		public string Key { get; set; }
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public string Subject { get; set; }
		public int DaysDuration { get; set; }
	}
}
