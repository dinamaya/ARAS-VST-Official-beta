using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class LatestRequestTransactionV
{
	public long TransactionId { get; set; }

	public long RequestId { get; set; }

	public string? RequestNumber { get; set; }

	public string AdjustmentTypeCode { get; set; } = null!;

	public string RequestorId { get; set; } = null!;

	public string RequestorFirstName { get; set; } = null!;

	public string RequestorLastName { get; set; } = null!;

	public DateTime DateRequested { get; set; }

	public string? ApproverId { get; set; }

	public string? ApproverFirstName { get; set; }

	public string? ApproverLastName { get; set; }

	public DateTime? DateApproved { get; set; }

	public string? ValidatorId { get; set; }

	public string? ValidatorFirstName { get; set; }

	public string? ValidatorLastName { get; set; }

	public DateTime? DateValidated { get; set; }

	public string CreatorId { get; set; } = null!;

	public string CreatorFirstName { get; set; } = null!;

	public string CreatorLastName { get; set; } = null!;

	public DateTime DateCreated { get; set; }

	public string Status { get; set; } = null!;
}
