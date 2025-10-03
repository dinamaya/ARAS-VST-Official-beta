using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IEmailService
	{
		Task<TaskResultDto> Test();
	}
}
