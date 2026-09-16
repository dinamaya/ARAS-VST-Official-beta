using ARAS.Blazor.Models.Entities;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface ITokenService
	{
		string GenerateToken(Account account, string role);
		string GenerateToken(Account account, IEnumerable<string> roles);
		string? GetToken();
	}
}
