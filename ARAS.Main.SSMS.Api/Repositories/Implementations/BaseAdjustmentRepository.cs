using ARAS.Main.SSMS.Api.App_Code.Globals;
using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Context;
using ARAS.Main.SSMS.Api.Models.Dtos;
using ARAS.Main.SSMS.Api.Repositories.Interfaces;
using ARAS.Main.SSMS.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Repositories.Implementations
{
	public class BaseAdjustmentRepository<TCreate> : IBaseAdjustmentRepository<TCreate>
	{
		private readonly MainDbContext _context;
		private readonly IBackgroundJobService _bgJobService;
		private readonly IAdjustmentRepository _adjustmentRepo;
		private readonly IRequestRepository _requestRepo;
		private readonly ITransactionRepository _transactionRepo;
		private readonly IRemarksRepository _remarksRepo;

		public BaseAdjustmentRepository(
			MainDbContext context, 
			IBackgroundJobService bgJobService, 
			IAdjustmentRepository adjustmentRepo, 
			IRequestRepository requestRepo, 
			ITransactionRepository transactionRepo, 
			IRemarksRepository remarksRepo)
		{
			_context = context;
			_bgJobService = bgJobService;
			_adjustmentRepo = adjustmentRepo;
			_requestRepo = requestRepo;
			_transactionRepo = transactionRepo;
			_remarksRepo = remarksRepo;
		}

		public async Task<long> Create(
			RequestCreationDto<AdjustmentRequestCreationDto<TCreate>> data, 
			string createdBy, 
			string adjustmentTypeCode,
			Func<TCreate, string, long, Task> createInvoiceCallBack)
		{
			Guards.ThrowInvalidOperationIf(string.IsNullOrEmpty(data.GroupCode), Exceptions.EMPTY_GROUP_CODE);
			Guards.ThrowInvalidOperationIf(!data.Model.Adjustments.Any(), Exceptions.EMPTY_CASHDISCOUNT_ROWS);

			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				string referenceNo = await _adjustmentRepo.GenerateReferenceNumber(data.GroupCode, adjustmentTypeCode);

				var request = new RequestCreateDto(referenceNo, adjustmentType.Id);
				var requestId = await _requestRepo.CreateAsync(request, createdBy);

				var transaction = new TransactionCreateDto(requestId, "Pending");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				foreach (var item in data.Model.Adjustments)
					await createInvoiceCallBack.Invoke(item, adjustmentType.Id, requestId);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(requestId);

				await _bgJobService.RunSendRequestPending(new ProceedEmailDto
				{
					RequestId = requestId.ToString(),
					RequestorName = data.CreatorFullName,
					AdjustmentType = adjustmentType.Name,
					RequestNumber = referenceNo,
					Status = "Pending",
					Timeline = timeline,
					ToEmail = data.Model.ToEmail,
				});

				await dbTransaction.CommitAsync();

				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task<long> Update(
			long requestId,
			RequestCreationDto<AdjustmentRequestCreationDto<TCreate>> data,
			string modifiedBy,
			string adjustmentTypeCode,
			string adjustmentRouteName,
			Func<TCreate, string, long, Task> updateInvoiceCallBack)
		{
			// Fetch the existing transaction by request Id
			// Create new Transaction with the status Pending
			// -- set the other details to the existing transaction
			// Fetch all Existing Adjustments and Invoice by RequestId
			// Deactivate the existing or previous adjustments and invoices
			// Insert both the updated and new adjustments and invoices

			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var requestRefNo = await _requestRepo.GetRequestNumberById(requestId);
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);

				var prevTimeline = await _transactionRepo.GetHistoryByRequestId(requestId);
				var prevCreatorRole = prevTimeline.LastOrDefault().AccountRole;

				var transaction = new TransactionCreateDto(requestId, "Pending");
				var transactId = await _transactionRepo.CreateAsync(transaction, modifiedBy);

				await _adjustmentRepo.DeactivateAllByRequestId(requestId);

				foreach (var item in data.Model.Adjustments)
					await updateInvoiceCallBack.Invoke(item, adjustmentType.Id, requestId);

				var updatedTimeline = await _transactionRepo.GetEmailHistoryByRequestId(requestId);
				var updateEmail = new UpdateEmailDto();

				if (prevCreatorRole.Equals("Approver"))
				{
					updateEmail = new UpdateEmailDto
					{
						RequestId = requestId.ToString(),
						RequestorName = data.CreatorFullName,
						AdjustmentType = adjustmentType.Name,
						RequestNumber = requestRefNo,
						Status = "Update / Resubmitted",
						ForAction = "For Approval",
						Role = prevCreatorRole,
						RoleGroup = "Manager / Supervisor",
						Timeline = updatedTimeline,
						ToEmail = data.Model.ToEmail,
						Endpoint = "approvals/" + adjustmentRouteName,
					};
				}
				else if (prevCreatorRole.Equals("Validator"))
				{
					updateEmail = new UpdateEmailDto
					{
						RequestId = requestId.ToString(),
						RequestorName = data.CreatorFullName,
						AdjustmentType = adjustmentType.Name,
						RequestNumber = requestRefNo,
						Status = "Update / Resubmitted",
						ForAction = "For Validation",
						Role = prevCreatorRole,
						RoleGroup = "FSG",
						Timeline = updatedTimeline,
						ToEmail = data.Model.ToEmail,
						Endpoint = "validations/" + adjustmentRouteName,
					};
				}
				else
					throw new InvalidCastException(Exceptions.INVALID_PREVIOUSCREATOR);

				await _bgJobService.RunSendRequestUpdated(updateEmail);
				await dbTransaction.CommitAsync();
				return requestId;
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Approve(RequestUpdateDto data, string createdBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				bool isApprovable = await _requestRepo.IsApprovable(data.RequestId);
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				Guards.ThrowInvalidOperationIf(!isApprovable, Exceptions.ALREADY_APPROVED);

				var transaction = new TransactionCreateDto(data.RequestId, "Approved");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(data.RequestId);
				var request = await _requestRepo.GetForEmailDetailsById(data.RequestId);

				await _bgJobService.RunSendRequestApproved(new ProceedEmailDto
				{
					RequestId = data.RequestId.ToString(),
					RequestorName = request.Creator,
					AdjustmentType = adjustmentType.Name,
					RequestNumber = request.RequestNumber,
					Status = "Approved",
					Timeline = timeline,
					ToEmail = data.ToEmail,
				});

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Decline(NegateRequestDto data, string createdBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				bool isDeclinable = await _requestRepo.IsDeclinable(data.RequestId);
				Guards.ThrowInvalidOperationIf(!isDeclinable, Exceptions.ALREADY_DECLINED);

				var transaction = new TransactionCreateDto(data.RequestId, "Declined");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var remarks = new RemarksCreateDto(transactId, data.Remarks);
				await _remarksRepo.CreateAsync(remarks, createdBy);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(data.RequestId);
				var request = await _requestRepo.GetForEmailDetailsById(data.RequestId);
				var latestUpdateDetails = timeline.LastOrDefault();

				await _bgJobService.RunSendRequestDeclined(new()
				{
					RequestId = data.RequestId.ToString(),
					AdjustmentType = adjustmentType.Name,
					RequestorName = request.Creator,
					RequestNumber = request.RequestNumber,
					Remarks = data.Remarks,
					Status = "Declined",
					Timeline = timeline,
					UpdatedBy = latestUpdateDetails.CreatorFullName,
					ToEmail = data.ToEmail,
				});

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Reject(NegateRequestDto data, string createdBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				bool isRejectable = await _requestRepo.IsRejectable(data.RequestId);
				Guards.ThrowInvalidOperationIf(!isRejectable, Exceptions.NOT_REJECTABLE);

				var transaction = new TransactionCreateDto(data.RequestId, "Rejected");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(data.RequestId);
				var request = await _requestRepo.GetForEmailDetailsById(data.RequestId);
				var latestUpdateDetails = timeline.LastOrDefault();

				await _bgJobService.RunSendRequestRejected(new()
				{
					RequestId = data.RequestId.ToString(),
					AdjustmentType = adjustmentType.Name,
					RequestorName = request.Creator,
					RequestNumber = request.RequestNumber,
					Remarks = data.Remarks,
					Status = "Rejected",
					Timeline = timeline,
					UpdatedBy = latestUpdateDetails.CreatorFullName,
					ToEmail = data.ToEmail,
				});

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}

		public async Task Validate(RequestUpdateDto data, string createdBy, string adjustmentTypeCode)
		{
			await using var dbTransaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var adjustmentType = await _adjustmentRepo.GetAdjustmentInfoByCode(adjustmentTypeCode);
				bool isValidatable = await _requestRepo.IsValidatable(data.RequestId);
				Guards.ThrowInvalidOperationIf(!isValidatable, Exceptions.ALREADY_VALIDATED);

				var transaction = new TransactionCreateDto(data.RequestId, "Validated");
				var transactId = await _transactionRepo.CreateAsync(transaction, createdBy);

				var timeline = await _transactionRepo.GetEmailHistoryByRequestId(data.RequestId);
				var request = await _requestRepo.GetForEmailDetailsById(data.RequestId);

				await _bgJobService.RunSendRequestValidated(new ProceedEmailDto
				{
					RequestId = data.RequestId.ToString(),
					RequestorName = request.Creator,
					AdjustmentType = adjustmentType.Name,
					RequestNumber = request.RequestNumber,
					Status = "Validated",
					Timeline = timeline,
					ToEmail = data.ToEmail,
				});

				await dbTransaction.CommitAsync();
			}
			catch
			{
				await dbTransaction.RollbackAsync();
				throw;
			}
		}
	}
}
