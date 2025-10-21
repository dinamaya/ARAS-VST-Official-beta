using ARAS.Main.SSMS.Api.Models.Complex;
using System.Security.Claims;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class UserClaimsExtension
	{
		public static string? GetClaim(this ClaimsPrincipal user, string claimType) =>
			user.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;

		public static string? GetIdentityClaim(this ClaimsPrincipal user, string claimType) =>
			user.Identities.FirstOrDefault().Claims.FirstOrDefault(c => c.Type == claimType)?.Value;

		public static AccountBasicInfo GetAccountBasicInfo(this ClaimsPrincipal user) => 
			new(
				GetClaim(user, ClaimTypes.PrimarySid) ?? "",
				GetClaim(user, ClaimTypes.GroupSid) ?? "",
				GetClaim(user, ClaimTypes.GivenName) + " "+ GetClaim(user, ClaimTypes.Surname)
			);
	}
}
