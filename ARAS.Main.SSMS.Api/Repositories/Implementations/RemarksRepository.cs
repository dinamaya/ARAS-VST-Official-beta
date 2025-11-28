using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class RemarksRepository : IRemarksRepository
	{
		private readonly MainDbContext _context;

		public RemarksRepository(MainDbContext context)
		{
			_context = context;
		}

		public async Task<string> CreateAsync(RemarksCreateDto data, string createdBy)
		{
			var date = DateTime.Now;
			var remarks = new TransactionRemarks
			{
				TransactionId = data.TransactionId,
				AttachmentName = null,
				Description = data.Remarks,
				
				CreatedBy = createdBy,
				DateCreated = date,
				ModifiedBy = createdBy,
				DateModified = date,
				IsActive = true
			};

			await _context.TransactionRemarks.AddAsync(remarks);
			await _context.SaveChangesAsync();

			return remarks.Id;
		}
	}
}
