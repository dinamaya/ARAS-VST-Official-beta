namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface ICreateMultipleRepository<TModel, TReturn>
	{
		Task<IEnumerable<TReturn>> CreateAsync(IEnumerable<TModel> data, string createdBy);
	}
}
