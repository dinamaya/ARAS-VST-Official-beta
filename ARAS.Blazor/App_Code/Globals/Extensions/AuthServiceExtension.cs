using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class AuthServiceExtension
	{
		public static bool IsInRole(this AccountDetailsDto accountDetails, string roleName) => accountDetails.AccountRole.Equals(roleName, StringComparison.CurrentCultureIgnoreCase);
	}
}
