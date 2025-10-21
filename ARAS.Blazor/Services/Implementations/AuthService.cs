using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using ARAS.Blazor.App_Code.Globals.Extensions;
using Azure;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ARAS.Blazor.Services.Implementations
{
	public class AuthService : IAuthService
	{

		private readonly CookieAuthStateProvider _authStateProvider;
		private Lazy<Task<AuthenticationState>> _authStateLazy;

		public AuthService(CookieAuthStateProvider authStateProvider)
		{
			_authStateProvider = authStateProvider;
			InitLazyState();
		}

		private async Task<ClaimsPrincipal> GetUser()
		{
			var authState = await GetAuthStateAsync();
			return authState!.User;
		}

		public async Task<AuthenticationState> GetAuthStateAsync() => await _authStateLazy.Value;

		public async Task<bool> IsAuthenticated()
		{
			var user = await GetUser();
			return user.Identity is not null && user.Identity.IsAuthenticated;
		}

		public async Task<bool> IsAuthenticated(string roleName)
		{
			var user = await GetUser();
			return user.Identity is not null && user.Identity.IsAuthenticated && user.IsInRole(roleName);
		}

		public async Task<string> GetRole()
		{
			var user = await GetUser();
			if(user.Identity is null || !user.Identity.IsAuthenticated)
				return string.Empty;

			return user.GetClaim(ClaimTypes.Role) ?? string.Empty;
		}

		public async Task<AccountDetailsDto> GetAccountDetails()
		{
			var user = await GetUser();
			if (user.Identity is null || !user.Identity.IsAuthenticated)
				return null;

			string role = user.GetClaim(ClaimTypes.Role) ?? "Unassigned";
			return new AccountDetailsDto()
			{
				FullName = user.GetClaim(JwtRegisteredClaimNames.GivenName) + " " + user.GetClaim(JwtRegisteredClaimNames.FamilyName),
				AccountRole =  role == "Ops" ? "Admin" : role,
				GroupCode = user.GetClaim(ClaimTypes.GroupSid) ?? string.Empty,
				Email = user.GetClaim(ClaimTypes.Email) ?? string.Empty
			};
		}


		public async Task<string> GetLastFirstName()
		{
			var user = await GetUser();
			if (user.Identity is null || !user.Identity.IsAuthenticated)
				return null;
			return user.GetClaim(JwtRegisteredClaimNames.FamilyName) + ", " + user.GetClaim(JwtRegisteredClaimNames.GivenName);
		}

		public void InitLazyState()
		{
			_authStateLazy = new Lazy<Task<AuthenticationState>>(
				() => _authStateProvider.GetAuthenticationStateAsync()
			);
		}
	}
}
