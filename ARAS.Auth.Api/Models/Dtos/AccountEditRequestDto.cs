namespace ARAS.Auth.Api.Models.Dtos
{
	public class AccountEditRequestDto
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string AccountRole { get; set; }
		public string? GroupCode { get; set; }
		public bool IsActive { get; set; }
	}
}
