using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class AuthServiceExtension
	{
		public static bool IsInRole(this string roleName, string expectedRole) =>
			string.Equals(roleName, expectedRole, StringComparison.CurrentCultureIgnoreCase);

		public static bool IsInRole(this AccountDetailsDto accountDetails, string roleName) =>
			accountDetails.AccountRole.IsInRole(roleName);

		public static bool IsCncApprover(this string roleName) =>
			roleName.IsInRole("CNC Approver");

		public static bool IsCncApprover(this AccountDetailsDto accountDetails) =>
			accountDetails.AccountRole.IsCncApprover();

		public static bool IsFsgValidator(this string roleName) =>
			roleName.IsInRole("FSG Validator");

		public static bool IsFsgValidator(this AccountDetailsDto accountDetails) =>
			accountDetails.AccountRole.IsFsgValidator();

		public static bool IsFsgApprover(this string roleName) =>
			roleName.IsInRole("FSG Approver");

		public static bool IsFsgApprover(this AccountDetailsDto accountDetails) =>
			accountDetails.AccountRole.IsFsgApprover();

		public static bool IsApprovalActor(this string roleName) =>
			roleName.IsCncApprover() || roleName.IsFsgValidator() || roleName.IsFsgApprover();

		public static bool IsApprovalActor(this AccountDetailsDto accountDetails) =>
			accountDetails.AccountRole.IsApprovalActor();
	}
}