using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class AdjustmentRepository : IAdjustmentRepository
	{
		private readonly MainDbContext _context;
		private readonly IInvoiceRepository _invoiceRepo;

		public AdjustmentRepository(MainDbContext context, IInvoiceRepository invoiceRepo)
		{
			_context = context;
			_invoiceRepo = invoiceRepo;
		}

		public async Task<long> CreateAsync(AdjustmentCreateDto data, string createdBy)
		{
			var date = DateTime.UtcNow;
			var adjustment = new Adjustment();

			adjustment.InvoiceId = data.InvoiceId;
			adjustment.RequestId = data.RequestId;
			adjustment.AdjustmentAmount = data.AdjustmentAmount;
			adjustment.AdjustmentTypeId = data.AdjustmentTypeId;
			adjustment.DiscountPercentage = data.DiscountPercentage;
			adjustment.ReasonCode = data.ReasonCode;
			adjustment.Remarks = data.Remarks;

			adjustment.DateCreated = date;
			adjustment.DateModified = date;
			adjustment.CreatedBy = createdBy;
			adjustment.ModifiedBy = createdBy;
			adjustment.IsActive = true;

			await _context.Adjustments.AddAsync(adjustment);
			await _context.SaveChangesAsync();

			return adjustment.Id;
		}

		public async Task<IEnumerable<Adjustment>> GetByRequestId(long requestId) => 
			await _context.Adjustments.Where(a => a.RequestId == requestId && a.IsActive).ToListAsync();

		public async Task<Adjustment> GetById(long id) => 
			await _context.Adjustments.Where(a => a.Id == id && a.IsActive).FirstOrDefaultAsync() ?? 
			throw new InvalidOperationException(Exceptions.NOTFOUND_ADJUSTMENT);
		
		public async Task DeactivateDetails(long id)
		{
			var adjustment = await GetById(id);
			var invoice = await _invoiceRepo.GetById(adjustment.Id);
			
			adjustment.IsActive = false;
			invoice.IsActive = false;

			_context.Adjustments.Update(adjustment);
			_context.Invoices.Update(invoice);
		}

		public async Task DeactivateAllByRequestId(long requestId)
		{
			var existingAdjustments = await GetByRequestId(requestId);

			foreach (var exAdjustment in existingAdjustments)
				await DeactivateDetails(exAdjustment.Id);
		}

		/// <summary>
		/// Generates the Reference Number for Request Creation
		/// </summary>
		/// <param name="groupCode">Account Group Code Ex. CC1, CC25</param>
		/// <param name="adjustmentTypeCode">Adjustment Type Code: CDR, WOR, APAR</param>
		/// <returns>
		/// <b>Format:</b> GroupCode-AdjustmentTypeCode-DAY_MONTH_YEAR-REQUEST_COUNT_INDEX_PER_GROUPCODE_DAY_MONTH_YEAR
		/// <br/>
		/// <b>Example:</b> 
		/// <list type="number">
		/// 	<item>CC1-CDR-31012025-<b>001</b>, CC1-CDR-31012025-<b>002</b>, CC1-CDR-31012025-<b>003</b> </item>
		///		<item><b>CC2</b>-CDR-31012025-001, <b>CC2</b>-CDR-31012025-002, <b>CC2</b>-CDR-31012025-003 </item>
		///		<item>CC2-CDR-<b>31022025</b>-001, CC2-CDR-<b>31032025</b>-001, CC2-CDR-<b>31042025</b>-001 </item>
		/// </list>
		/// </returns>
		public async Task<string> GenerateReferenceNumber(string groupCode, string adjustmentTypeCode)
		{
			var today = DateOnly.FromDateTime(DateTime.Now.Date);
			string grpCode = groupCode.ToUpper();
			string adjCode = adjustmentTypeCode.ToUpper();

			// Get all requests for same groupCode and same date
			var requests = _context.VwRequestsNumberSources.Where(r =>
				r.GroupCode == grpCode &&
				r.AdjustmentTypeCode == adjCode &&
				r.RequestDate == today
			);

			int nextIndex = (await requests.CountAsync()) + 1;

			string datePart = today.ToString(Formats.Date.REFERNUMBER);
			string indexPart = nextIndex.ToString("D3");

			return $"{groupCode}-{adjCode}-{datePart}-{indexPart}";
		}

		public async Task<AdjustmentBasicInfoDto> GetAdjustmentInfoByCode(string adjustmentTypeCode) => 
			await _context.AdjustmentTypes
			.Where(x => x.Code.Equals(adjustmentTypeCode))
			.Select(x => new AdjustmentBasicInfoDto(x.Id, x.Name)).FirstOrDefaultAsync() ?? 
			throw new InvalidOperationException(Exceptions.NOTFOUND_ADJUSTMENTTYPE);

		public async Task<IEnumerable<RequestAdjustmentsV>> GetAllByRequestIdAndCode(long requestId, string adjustmentTypeCode)
		{
			return await _context.VwRequestAdjustments
				.Where(r => r.AdjustmentTypeCode == adjustmentTypeCode && r.RequestId == requestId)
				.ToListAsync();
		}
	}
}
