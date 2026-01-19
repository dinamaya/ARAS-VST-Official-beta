namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface ICreateMultipleRepository<TModel> where TModel : class
	{
		Task CreateAsync(IEnumerable<TModel> data, string createdBy);
	}
}
