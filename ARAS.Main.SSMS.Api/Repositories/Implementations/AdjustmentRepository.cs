using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class AdjustmentRepository : ICreateRepository<AdjustmentCreateDto>
	{
		public string InsertedId { get; set; }

		public Task CreateAsync(AdjustmentCreateDto data, string createdBy)
		{
			throw new NotImplementedException();
		}
	}
}
