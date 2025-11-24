namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IInputValidatorRepository<TValidation>
	{
		Task<bool> IsValid(TValidation inputValidation);
	}
}
