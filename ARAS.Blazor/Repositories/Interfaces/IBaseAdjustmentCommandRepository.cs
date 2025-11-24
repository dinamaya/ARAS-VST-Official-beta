using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IBaseAdjustmentCommandRepository<TRow, TCreate, TValidation> : 
		ICreateStatusRepository<TRow>,
		IAdjustmentReaderRepository<TRow>,
		IRequestSubmissionReaderRepository,
		IInputValidatorRepository<TValidation>
	{
	}
}
