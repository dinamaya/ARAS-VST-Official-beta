using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;
using ARAS.Blazor.Services.Interfaces;
using System.Linq;

namespace ARAS.Blazor.Services.Implementations
{
    public class ARInvoiceOffsettingService : IARInvoiceOffsettingService
    {
        private readonly IBaseService _baseService;
        private readonly IConfigService _configService;
        private readonly INoteService _noteService;

        public ARInvoiceOffsettingService(IConfigService configService,
            IBaseService baseService,
            INoteService noteService)
        {
            _baseService = baseService;
            _configService = configService;
            _noteService = noteService;
        }

        private static List<ARInvoiceOffsettingCreateDto> ToCreateDto(IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingCNDetailsDto> cnRows)
        {
            var arRequests = arRows.Select(r => new ARInvoiceOffsettingCreateDto()
            {
                InvoiceAmount = r.InvoiceAmount,
                InvoiceDate = DateTime.Parse(r.InvoiceDate),
                InvoiceNumber = r.InvoiceNumber,
                CustomerName = r.CustomerName,
                CustomerNumber = r.CustomerNumber,
                Type = "AR",
            });

            var cnRequests = cnRows.Select(r => new ARInvoiceOffsettingCreateDto()
            {
                InvoiceAmount = r.InvoiceAmount,
                InvoiceDate = DateTime.Parse(r.InvoiceDate),
                InvoiceNumber = r.InvoiceNumber,
                CustomerName = r.CustomerName,
                CustomerNumber = r.CustomerNumber,
                Type = "CN",
            });

            return cnRequests.Concat(arRequests).ToList();
        }

        public async Task Create(IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingCNDetailsDto> cnRows)
        {
            var requestsDto = ToCreateDto(arRows, cnRows);

            var createResult = await _baseService.SendAsync<long>(new RequestDto<IEnumerable<ARInvoiceOffsettingCreateDto>>()
            {
                ApiType = ApiType.POST,
                URL = _configService.GetRequestsUrl("invoice/ari"),
                Data = requestsDto
            },
                onSuccessSendCallBack: (resp) =>
                {
                    Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to create request" + resp.Message);
                    return Task.CompletedTask;
                }
            );

        }

        Task IARInvoiceOffsettingService.Update(long requestId, IEnumerable<ARInvoiceOffsettingRowDto> rows, IEnumerable<NoteRowDto> notes)
        {
            throw new NotImplementedException();
        }

        Task IARInvoiceOffsettingService.Approve(long requestId, IEnumerable<NoteRowDto> notes)
        {
            throw new NotImplementedException();
        }

        Task IARInvoiceOffsettingService.Validate(long requestId, IEnumerable<NoteRowDto> notes)
        {
            throw new NotImplementedException();
        }

        Task IARInvoiceOffsettingService.Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes)
        {
            throw new NotImplementedException();
        }

        Task IARInvoiceOffsettingService.Reject(NegateRequestDto createReject, IEnumerable<NoteRowDto> notes)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<ARInvoiceOffsettingRowDto>> IARInvoiceOffsettingService.GetAdjustments(long requestId)
        {
            throw new NotImplementedException();
        }

        Task<string> IAdjustmentReaderRepository<ARInvoiceOffsettingRowDto>.GetActivityByCode()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<ARInvoiceOffsettingRowDto>> IAdjustmentReaderRepository<ARInvoiceOffsettingRowDto>.GetAdjustments(long requestId)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TransactionRequestRowDto>> IRequestSubmissionReaderRepository.GetSubmissions()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TransactionRequestRowDto>> IRequestSubmissionReaderRepository.GetApprovals()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TransactionRequestRowDto>> IRequestSubmissionReaderRepository.GetValidations()
        {
            throw new NotImplementedException();
        }

        Task<bool> IInputValidatorRepository<object>.IsValid(object inputValidation)
        {
            throw new NotImplementedException();
        }
    }
}
