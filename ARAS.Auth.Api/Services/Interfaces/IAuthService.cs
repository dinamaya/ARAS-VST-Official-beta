using ARAS.Auth.Api.Models.Dtos;

namespace ARAS.Auth.Api.Services.Interfaces
{
	public interface IAuthService
	{
		Task<string> SignInAsync(AccountSignInRequestDto signInRequest);
	}
}
