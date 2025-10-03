namespace ARAS.Auth.Api.Models.Dtos
{
	public class AccountRowDto
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string CreatedBy { get; set; }
		public string AccountRole { get; set; }
		public bool IsActive { get; set; }
	}
}
