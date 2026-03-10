using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class AuthServiceExtension
	{
		public static bool IsInRole(this AccountDetailsDto accountDetails, string roleName) =>
			string.Equals(accountDetails.AccountRole, roleName, StringComparison.CurrentCultureIgnoreCase);

		public static bool IsCncApprover(this AccountDetailsDto accountDetails) =>
			string.Equals(accountDetails.AccountRole, "Approver", StringComparison.CurrentCultureIgnoreCase) ||
			string.Equals(accountDetails.AccountRole, "CNC Approver", StringComparison.CurrentCultureIgnoreCase);
	}
}