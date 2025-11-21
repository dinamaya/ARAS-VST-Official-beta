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

        public async Task<IEnumerable<TransactionRequestRowDto>> GetSubmissions()
        {
            var response = await _baseService.SendAsync<IEnumerable<TransactionRequestRowDto>>(
                new RequestDto()
                {
                    URL = _configService.GetAPAROffsetsUrl("submissions"),
                },
                onSuccessSendCallBack: async (resp) =>
                {
                    await Task.Run(() =>
                    {
                        Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the submitted requests");
                    });
                });

            return response.Result;
        }

        public async Task<TransactionRequestRowDto> GetRequestDetails(long requestId)
        {
            var response = await _baseService.SendAsync<TransactionRequestRowDto>(new RequestDto()
            {
                URL = _configService.GetRequestsUrl($"aar/{requestId}"),
            },
                onSuccessSendCallBack: async (resp) =>
                {
                    await Task.Run(() =>
                    {
                        Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the request");
                    });
                });

            return response.Result;
        }

        public async Task<Tuple<IEnumerable<APAROffsetAPRowDto>, IEnumerable<APAROffsetARRowDto>>> GetAdjustments(long requestId)
        {
            var apResponse = await _baseService.SendAsync<IEnumerable<APAROffsetAPRowDto>>(new RequestDto()
            {
                URL = _configService.GetAdjustmentsUrl($"aar/{requestId}/ap"),
            },
                onSuccessSendCallBack: async (resp) =>
                {
                    await Task.Run(() =>
                    {
                        Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the AP adjustments");
                    });
                });

            var arResponse = await _baseService.SendAsync<IEnumerable<APAROffsetARRowDto>>(new RequestDto()
            {
                URL = _configService.GetAdjustmentsUrl($"aar/{requestId}/ar"),
            },
                onSuccessSendCallBack: async (resp) =>
                {
                    await Task.Run(() =>
                    {
                        Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to fetch the AR adjustments");
                    });
                });

            return new Tuple<IEnumerable<APAROffsetAPRowDto>, IEnumerable<APAROffsetARRowDto>>(apResponse.Result, arResponse.Result);
        }


        public async Task Update(long requestId, IEnumerable<APAROffsetAPRowDto> apRows, IEnumerable<APAROffsetARRowDto> arRows, IEnumerable<NoteRowDto> notes)
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
            var adjustmentRequestUpdate = new AdjustmentRequestCreationDto<APAROffsetCreateDto>(requestsDto, emails);

            await _baseService.SendAsync<string>(
                new RequestDto<AdjustmentRequestCreationDto<APAROffsetCreateDto>>()
                {
                    ApiType = ApiType.POST,
                    URL = _configService.GetAPAROffsetsUrl($"update/{requestId}"),
                    Data = adjustmentRequestUpdate
                },
                onSuccessSendCallBack: async (resp) =>
                {
                    await Task.Run(() =>
                    {
                        Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to update ap-ar offset request");
                    });
                }
            );

            await _noteService.Create(requestId, notes);
        }

        public async Task Decline(NegateRequestDto createDecline, IEnumerable<NoteRowDto> notes)
        {
            createDecline.ToEmail = await _emailService.GetAll();

            await _baseService.SendAsync<string>(
                new RequestDto<NegateRequestDto>()
                {
                    ApiType = ApiType.POST,
                    URL = _configService.GetDeclinesUrl("aar"),
                    Data = createDecline
                },
                onSuccessSendCallBack: async (resp) =>
                {
                    await Task.Run(() =>
                    {
                        Guards.ThrowInvalidOperationIf(!resp.IsSuccess, "Failed to DECLINE the current request");
                        Guards.ThrowInvalidOperationIf(!(resp.IsSuccess && resp.Result == "Success"), "Failed to DECLINE the current request");
                    });
                });

            await _noteService.Create(createDecline.RequestId, notes);
        }
    }
}
