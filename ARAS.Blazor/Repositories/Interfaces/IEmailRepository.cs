namespace ARAS.Blazor.Repositories.Interfaces
{
	public interface IEmailRepository
	{
		Task<IEnumerable<string>> GetAll();
		Task<IEnumerable<string>> GetApprovers();
		Task<IEnumerable<string>> GetValidators();
	}
}
