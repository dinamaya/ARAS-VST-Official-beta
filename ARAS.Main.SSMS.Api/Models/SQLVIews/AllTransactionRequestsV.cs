using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class AllTransactionRequestsV
{
	public long TransactionId { get; set; }

	public long RequestId { get; set; }

	public string? RequestNumber { get; set; }

	public string? RequestorId { get; set; }

	public string RequestorFirstName { get; set; } = null!;

	public string RequestorLastName { get; set; } = null!;

	public DateTime? DateRequested { get; set; }

	public string? ApproverId { get; set; }

	public string? ApproverFirstName { get; set; }

	public string? ApproverLastName { get; set; }

	public DateTime? DateApproved { get; set; }

	public string? ValidatorId { get; set; }

	public string? ValidatorFirstName { get; set; }

	public string? ValidatorLlastName { get; set; }

	public DateTime? DateValidated { get; set; }

	public string? CheckerId { get; set; }

	public string? CheckerFirstName { get; set; }

	public string? CheckerLastName { get; set; }

	public DateTime? DateChecked { get; set; }

	public DateTime TransactionDateCreated { get; set; }

	public string Status { get; set; } = null!;
}
