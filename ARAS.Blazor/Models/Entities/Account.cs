using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ARAS.Blazor.Models.Entities
{
	public class Account : IdentityUser, IAuditableByUser
	{
		public Account() => base.Id = Utils.Security.GenerateExtendedGuid("ACC", 2);
		public string SecurityHash { get; set; } = Utils.Security.GenerateExtendedGuid("", 2);

		public string OpenId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string? GroupCode { get; set; }

		public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime DateModified { get; set; }
		public bool IsActive { get; set; }
	}
}
