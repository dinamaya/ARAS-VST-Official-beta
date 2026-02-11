using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ARAS.Blazor.App_Code.Globals.Extensions;
using Microsoft.AspNetCore.Authorization;


namespace ARAS.Main.SSMS.Api.Controllers
{
	[Route("api/transactions")]
	[ApiController]
	public class TransactionsController : ControllerBase
	{
		private readonly ITransactionRepository _transactionRepo;
		private readonly ILogger<TransactionsController> _logger;

		public TransactionsController(ITransactionRepository transactionRepo, ILogger<TransactionsController> logger)
		{
			_transactionRepo = transactionRepo;
			_logger = logger;
		}

		[HttpGet("history/{requestId:long}")]
		public async Task<ResponseDto<IEnumerable<TransactionHistoryDto>>> GetById(long requestId)
		{
			var response = new ResponseDto<IEnumerable<TransactionHistoryDto>>();
			try
			{
				response.Result = await _transactionRepo.GetHistoryByRequestId(requestId);
				response.Message = $"Successfully fetched the history of the request {requestId}";
				return response;
			}
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return response.Failed(ex.Message);
            }
        }

        [HttpGet("latest"), Authorize]
        public async Task<ResponseDto<IEnumerable<TransactionHistoryDto>>> GetLatest()
        {
            var response = new ResponseDto<IEnumerable<TransactionHistoryDto>>();
            try
            {
                var accountInfo = User.GetAccountBasicInfo();
                response.Result = await _transactionRepo.GetLatestTransactions(accountInfo.Id, 6);
                response.Message = "Successfully fetched latest transactions";
                return response;
            }
            catch (Exception ex) 
			{ 
				_logger.LogError(ex.Message);
				return response.Failed(ex.Message);
			}
		}
	}
}
