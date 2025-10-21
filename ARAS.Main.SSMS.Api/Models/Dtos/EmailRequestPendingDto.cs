using ARAS.Main.SSMS.Api.Models.Entities;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class EmailRequestPendingDto : IBaseEmail
	{
		public RequestPendingDto RequestPendingDetails { get; set; }
		public string BaseUrl { get; set; }

		public EmailRequestPendingDto(RequestPendingDto requestPendingDetails, string baseUrl)
		{
			RequestPendingDetails = requestPendingDetails;
			BaseUrl = baseUrl;
		}
	}
}
