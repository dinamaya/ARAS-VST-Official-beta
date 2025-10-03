using System.Security.Claims;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class UserClaimsExtension
	{
		public static string? GetClaim(this ClaimsPrincipal user, string claimType) =>
			user.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;

		public static string? GetIdentityClaim(this ClaimsPrincipal user, string claimType) =>
			user.Identities.FirstOrDefault().Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
	}
}
