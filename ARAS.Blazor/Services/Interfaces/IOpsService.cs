using ARAS.Blazor.Models.DTOs;
using System.Threading.Tasks;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IOpsService
	{
		Task<IEnumerable<AccountRowDto>> GetAccounts();
		Task<AccountEditRequestDto> GetAccountById(string id);
		Task<IEnumerable<DropdownOptionDto>> GetRoles();
		Task<bool> Update(AccountEditRequestDto data);
	}
}
