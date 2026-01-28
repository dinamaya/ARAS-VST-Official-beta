using ARAS.Main.SSMS.Api.Models.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IUpdateRepository<TModel, TReturnId> where TModel : class
	{
		Task<TReturnId> UpdateAsync(long requestId, TModel data, string modifiedBy);
	}
}
