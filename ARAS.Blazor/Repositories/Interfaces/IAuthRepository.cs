using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IAuthRepository
	{
		Task<string> SignInAsync(AccountSignInRequestDto signInRequest);
		Task<string> GetSecurityHashByOId(string oid);
	}
}
