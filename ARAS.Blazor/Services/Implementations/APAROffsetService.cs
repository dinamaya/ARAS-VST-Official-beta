using ARAS.Blazor.App_Code.Globals;
using ARAS.Blazor.App_Code.Globals.Enums;
using ARAS.Blazor.Models.DTOs;
using ARAS.Blazor.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Radzen;

namespace ARAS.Blazor.Services.Implementations
{
    public class APAROffsetService : IAPAROffsetService
    {
        private readonly IBaseService _baseService;
        private readonly IConfigService _configService;
        private readonly IEmailService _emailService;
        private readonly INoteService _noteService;
        private readonly DialogService _dialogService;

        public APAROffsetService(IBaseService baseService, IConfigService configService, IEmailService emailService, INoteService noteService, DialogService dialogService)
        {
            _baseService = baseService;
            _configService = configService;
            _emailService = emailService;
            _noteService = noteService;
            _dialogService = dialogService;
        }

        public async Task Create(IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes)
        {
            var apRequests = apRows.Select(r => new APAROffsetCreateDto()
            {
                InvoiceId = r.InvoiceNumber,
                Amount = r.InvoiceAmount,
                Type = "AP",
                InvoiceDate = r.InvoiceDate,
                CustomerName = r.CustomerName,
                CustomerNumber = r.CustomerNumber
            });

            var arRequests = arRows.Select(r => new APAROffsetCreateDto()
            {
                InvoiceId = r.InvoiceNumber,
                Amount = r.Amount,
                Type = "AR",
                InvoiceDate = DateTime.Now,
                CustomerName = "",
                CustomerNumber = ""
            });

            var requestsDto = apRequests.Concat(arRequests).ToList();

            var emails = await _emailService.GetApprovers();
            var adjustmentRequestCreation = new AdjustmentRequestCreationDto<APAROffsetCreateDto>(requestsDto, emails);

            var createResult = await _baseService.SendAsync<long>(new RequestDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>>()
            {
                ApiType = ApiType.POST,
                URL = _configService.GetAPAROffsetsUrl(),
                Data = adjustmentRequestCreation
            });

            await _noteService.Create(createResult.Result, notes);
        }
    }
}
