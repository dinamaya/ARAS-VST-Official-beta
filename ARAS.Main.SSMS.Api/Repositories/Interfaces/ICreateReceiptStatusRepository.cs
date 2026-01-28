using ARAS.Main.SSMS.Api.Models.Dtos;

namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
    public interface ICreateReceiptStatusRepository<TCreate, TReturn> where TCreate : class
	{
		Task<TReturn> Create(RequestCreationDto<IEnumerable<TCreate>> data, string createdBy);
	}
}
