using ARAS.Blazor.Models.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IAuthService
	{
		Task<bool> IsAuthenticated();
		Task<bool> IsAuthenticated(string roleName);
		Task<AuthenticationState> GetAuthStateAsync();
		Task<string> GetRole();
		Task<AccountDetailsDto> GetAccountDetails();
		Task<string> GetLastFirstName();
		void InitLazyState();
	}
}
