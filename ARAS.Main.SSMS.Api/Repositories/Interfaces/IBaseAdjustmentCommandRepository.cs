using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using ARAS.Main.SSMS.Api.Repositories.Implementations;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IBaseAdjustmentCommandRepository<TRow, TCreate, TValidation> : 
		ICreateStatusRepository, 
		ICreateRepository<RequestCreationDto<TCreate>, long>,
		IUpdateRepository<RequestCreationDto<TCreate>, long>,
		IAdjustmentReaderRepository<TRow>,
		IInputValidatorRepository<TValidation>
	{
	}
}
