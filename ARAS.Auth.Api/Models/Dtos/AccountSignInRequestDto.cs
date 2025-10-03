namespace ARAS.Auth.Api.Models.Dtos
{
	public class AccountSignInRequestDto
	{
		public string OpenId { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
