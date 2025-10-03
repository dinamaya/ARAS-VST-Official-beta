using ARAS.Auth.Api.Models.Dtos;

namespace ARAS.Auth.Api.Services.Interfaces
{
	public interface IAccountService
	{
		Task<IEnumerable<AccountRowDto>> GetAll();
		Task<AccountEditRequestDto> GetById(string id);
		Task<IEnumerable<DropdownOptionDto>> GetAllRoleOptions();
		Task Update(AccountEditRequestDto request);
	}
}
