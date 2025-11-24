using ARAS.Blazor.App_Code.Globals.Extensions;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Models.Entities;
using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ARAS.Blazor.Services.Implementations
{
	public class AuthService : IAuthService
	{
		private readonly CookieAuthStateProvider _authStateProvider;
		private readonly UserManager<Account> _userManager;
		private Lazy<Task<AuthenticationState>> _authStateLazy;

		public AuthService(CookieAuthStateProvider authStateProvider, UserManager<Account> usermanager)
		{
			_authStateProvider = authStateProvider;
			_userManager = usermanager;
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
			if (user.Identity is null || !user.Identity.IsAuthenticated)
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
				AccountRole = role == "Ops" ? "Admin" : role,
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

		public async Task<bool> IsAccountSecurityHashValid()
		{
			var user = await GetUser();
			if (user.Identity == null || !user.Identity.IsAuthenticated)
				return true;

			var email = user.Identity.Name;
			var tokenHash = user.GetClaim(ClaimTypes.Hash);

			if (email == null || tokenHash == null)
				return true;

			var account = await _userManager.FindByEmailAsync(email);
			if (account == null)
				return false;

			if (user == null || account.SecurityHash != tokenHash)
				return false;

			return account.SecurityHash == tokenHash;
		}
	}
}
