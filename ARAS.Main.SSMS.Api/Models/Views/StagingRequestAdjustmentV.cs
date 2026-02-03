using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.Views;

public partial class StagingRequestAdjustmentV
{
	public long AdjustmentId { get; set; }

	public long RequestId { get; set; }

	public string CustomerNumber { get; set; } = null!;

	public string InvoiceNumber { get; set; } = null!;

	public double AdjustmentAmount { get; set; }

	public DateTime InvoiceDate { get; set; }

	public DateTime DateApplied { get; set; }

	public string Activity { get; set; } = null!;

	public string ReasonCode { get; set; } = null!;

	public string Remarks { get; set; } = null!;

	public string CustomerName { get; set; } = null!;
}
