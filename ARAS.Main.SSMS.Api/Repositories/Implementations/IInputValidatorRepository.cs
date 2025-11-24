namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public interface IInputValidatorRepository<TValidation>
	{
		Task<bool> IsValid(TValidation inputValidation);
	}
}
