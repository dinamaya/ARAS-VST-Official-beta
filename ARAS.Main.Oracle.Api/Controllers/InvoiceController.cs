using ARAS.Main.Oracle.Api.Models.Dtos;
using ARAS.Main.Oracle.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ARAS.Main.Oracle.Api.Controllers
{
	[Route("api/invoice")]
	[ApiController]
	public class InvoiceController : ControllerBase
	{
		private readonly ILogger<InvoiceController> _logger;
		private readonly IInvoiceRepository _invoiceRepo;

		public InvoiceController(ILogger<InvoiceController> logger, IInvoiceRepository invoiceRepo)
		{
			_logger = logger;
			_invoiceRepo = invoiceRepo;
		}

		[HttpGet("no/{invoiceNo}")]
		public async Task<ResponseDto<InvoiceDetailsDto>> GetInvoiceNumbers(string invoiceNo)
		{
			ResponseDto<InvoiceDetailsDto> _response = new();
			try
			{
				_response.Result = await _invoiceRepo.GetInvoiceNo(invoiceNo);
				return _response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				return _response.Failed(ex.Message);
			}
		}
	}
}
