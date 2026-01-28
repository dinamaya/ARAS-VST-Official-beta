using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IBaseAdjustmentCommandRepository<TRow, TCreate, TValidation> : 
		IAdjustmentReaderRepository<TRow>,
		IRequestSubmissionReaderRepository,
		IInputValidatorRepository<TValidation>
	{
	}
}
