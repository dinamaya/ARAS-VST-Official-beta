using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Implementations;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;

namespace ARAS.Main.SSMS.Api.Services.Implementations
{
	public class AdjustmentService : IAdjustmentService
	{
		private readonly ICashDiscountRepository _cashDiscountRepo;
		private readonly IBankChargeRepository _bankChargeRepo;

		public AdjustmentService(ICashDiscountRepository cashDiscountRepo, IBankChargeRepository bankChargeRepo)
		{
			_cashDiscountRepo = cashDiscountRepo;
			_bankChargeRepo = bankChargeRepo;
		}

		/// <summary>
		/// <para>Add the repositories here</para> 
		/// <para>Throws Invalid Operation Exception when the adjustment type code incorrect or does not exist</para> 
		/// </summary>
		/// <param name="adjustmentTypeCode">
		/// </param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		private ICreateStatusRepository AdjustmentTypeCheck(string adjustmentTypeCode)
		{
			adjustmentTypeCode = adjustmentTypeCode.ToLower();
			return adjustmentTypeCode switch
			{
				"cdr" => _cashDiscountRepo,
				"bca" => _bankChargeRepo,
				_ => throw new InvalidOperationException(Exceptions.NOTFOUND_ADJUSTMENTTYPE)
			};
		}

		public async Task Approve(RequestUpdateDto data, string createdBy, string adjustmentTypeCode)
		{
			ICreateStatusRepository adjustmentService = AdjustmentTypeCheck(adjustmentTypeCode);
			await adjustmentService.ApproveAsync(data, createdBy);
		}

		public async Task Decline(NegateRequestDto createDecline, string createdBy, string adjustmentTypeCode)
		{
			ICreateStatusRepository adjustmentService = AdjustmentTypeCheck(adjustmentTypeCode);
			await adjustmentService.DeclineAsync(createDecline, createdBy);
		}

		public async Task Reject(NegateRequestDto createDecline, string createdBy, string adjustmentTypeCode)
		{
			ICreateStatusRepository adjustmentService = AdjustmentTypeCheck(adjustmentTypeCode);
			await adjustmentService.RejectAsync(createDecline, createdBy);
		}

		public async Task Validate(RequestUpdateDto data, string createdBy, string adjustmentTypeCode)
		{
			ICreateStatusRepository adjustmentService = AdjustmentTypeCheck(adjustmentTypeCode);
			await adjustmentService.ValidateAsync(data, createdBy);
		}
	}
}
