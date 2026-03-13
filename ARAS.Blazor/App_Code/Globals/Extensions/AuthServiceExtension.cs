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

		public static bool CanUpdateApprovalStatus(this string roleName, string? status) =>
			(roleName, status) switch
			{
				(var role, "For CNC Approval") when role.IsCncApprover() => true,
				(var role, "For FSG Validation") when role.IsFsgValidator() => true,
				(var role, "For FSG Approval") when role.IsFsgApprover() => true,
				_ => false
			};

		public static string GetApprovalQueueStatus(this string roleName) =>
			roleName switch
			{
				var role when role.IsCncApprover() => "For CNC Approval",
				var role when role.IsFsgValidator() => "For FSG Validation",
				var role when role.IsFsgApprover() => "For FSG Approval",
				_ => string.Empty
			};

		public static bool CanUpdateApprovalStatus(this AccountDetailsDto accountDetails, string? status) =>
			accountDetails.AccountRole.CanUpdateApprovalStatus(status);
	}
}
