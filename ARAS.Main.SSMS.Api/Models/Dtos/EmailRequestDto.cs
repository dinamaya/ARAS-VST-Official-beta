using ARAS.Main.SSMS.Api.Models.Entities;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class EmailRequestDto<TEmailModel> : IBaseEmail
	{
		public TEmailModel RequestDetails { get; set; }
		public string BaseUrl { get; set; }

		public EmailRequestDto(TEmailModel requestPendingDetails, string baseUrl)
		{
			RequestDetails = requestPendingDetails;
			BaseUrl = baseUrl;
		}
	}
}
