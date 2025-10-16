using ARAS.Main.SSMS.Api.Models.Entities;
using System.Security.Cryptography;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IReadSingleRepository<TModel, TId>
	{
		Task<TModel> GetById(TId id);
	}
}
