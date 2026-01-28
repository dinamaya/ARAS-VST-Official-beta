using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IAccountRepository
	{
		Task<IEnumerable<AccountRowDto>> GetAll();
		Task<AccountEditRequestDto> GetById(string id);
		Task<IEnumerable<DropdownOptionDto>> GetAllRoleOptions();
		Task Update(AccountEditRequestDto request);
	}
}
