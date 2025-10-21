namespace ARAS.Blazor.Services.Interfaces
{
	public interface IEmailService
	{
		Task<IEnumerable<string>> GetAll();
		Task<IEnumerable<string>> GetApprovers();
		Task<IEnumerable<string>> GetValidators();
	}
}
