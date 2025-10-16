namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface ICreateRepository<TModel, TReturnId> where TModel : class
	{
		Task<TReturnId> CreateAsync(TModel data, string createdBy);
	}
}
