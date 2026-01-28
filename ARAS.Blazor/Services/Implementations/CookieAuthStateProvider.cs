using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ARAS.Blazor.Services.Implementations
{
	public class CookieAuthStateProvider : AuthenticationStateProvider
	{
		private readonly ITokenService _tokenService;

		public CookieAuthStateProvider(ITokenService tokenService)
		{
			_tokenService = tokenService;
		}

		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var token = _tokenService.GetToken();
			var state = BuildAuthenticationStateFromToken(token);
			return Task.FromResult(state);

		}

		private AuthenticationState BuildAuthenticationStateFromToken(string token)
		{
			if (string.IsNullOrEmpty(token))
				return AnonymouseState();

			try
			{
				var handler = new JwtSecurityTokenHandler();
				JwtSecurityToken jwt = handler.ReadJwtToken(token);

				var authClaims = new List<Claim>();

				foreach (var c in jwt.Claims)
				{
					if (string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(c.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
					{
						authClaims.Add(new Claim(ClaimTypes.Role, c.Value));
						continue;
					}

					if (string.Equals(c.Type, JwtRegisteredClaimNames.Sub, StringComparison.OrdinalIgnoreCase))
					{
						authClaims.Add(new Claim(ClaimTypes.NameIdentifier, c.Value));
						continue;
					}

					if (string.Equals(c.Type, JwtRegisteredClaimNames.Name, StringComparison.OrdinalIgnoreCase))
					{
						authClaims.Add(new Claim(ClaimTypes.Name, c.Value));
						continue;
					}

					if (string.Equals(c.Type, JwtRegisteredClaimNames.Email, StringComparison.OrdinalIgnoreCase) ||
						string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(c.Type, "upn", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(c.Type, "preferred_username", StringComparison.OrdinalIgnoreCase))
					{
						authClaims.Add(new Claim(ClaimTypes.Email, c.Value));
						continue;
					}

					if(string.Equals(c.Type, JwtRegisteredClaimNames.GivenName)){
						authClaims.Add(new Claim(JwtRegisteredClaimNames.GivenName, c.Value));
						continue;
					}

					if(string.Equals(c.Type, JwtRegisteredClaimNames.FamilyName)){
						authClaims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, c.Value));
						continue;
					}


					if (string.Equals(c.Type, ClaimTypes.Hash))
					{
						authClaims.Add(new Claim(ClaimTypes.Hash, c.Value));
						continue;
					}

					authClaims.Add(new Claim(c.Type, c.Value));
				}


				var identity = new ClaimsIdentity(authClaims, "jwt");
				var user = new ClaimsPrincipal(identity);
				return new AuthenticationState(user);
			}
			catch (Exception ex)
			{
				return AnonymouseState();
			}
		}

		private AuthenticationState AnonymouseState()
		{
			var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
			return new AuthenticationState(anonymous);
		}

		private AuthenticationState AuthenticatedState(string token)
		{
			var handler = new JwtSecurityTokenHandler();
			var jwt = handler.ReadJwtToken(token);

			var identity = new ClaimsIdentity(jwt.Claims, "jwt");
			var user = new ClaimsPrincipal(identity);

			return new AuthenticationState(user);
		}

		public void NotifyUserAuthentication(string token)
		{
			NotifyAuthenticationStateChanged(Task.FromResult(this.AuthenticatedState(token)));
		}

		public void NotifyUserLogout()
		{
			NotifyAuthenticationStateChanged(Task.FromResult(this.AnonymouseState()));
		}
	}
}
