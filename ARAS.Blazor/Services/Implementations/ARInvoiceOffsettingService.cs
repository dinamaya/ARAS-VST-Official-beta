using ARAS.Blazor.Services.Interfaces;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Repositories.Interfaces;

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

        private static readonly Func<ARInvoiceOffsettingRowDto, ARInvoiceOffsettingCreateDto> Map = (row) => new()
        {
            InvoiceAmount = row.InvoiceAmount,
            InvoiceDate = DateTime.Parse(row.InvoiceDate),
            InvoiceNumber = row.InvoiceNumber,
            CustomerName = row.CustomerName,
            CustomerNumber = row.CustomerNumber,
            Remarks = row.Remarks,
        };

        Task IARInvoiceOffsettingService.Create(IEnumerable<ARInvoiceOffsettingRowDto> arRows, IEnumerable<ARInvoiceOffsettingRowDto> cnRows, IEnumerable<NoteRowDto> notes)
        {
            throw new NotImplementedException();
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
