using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class RequestAdjustmentsV
{
	public string AdjustmentActivity { get; set; } = null!;

	public string? RequestNumber { get; set; }

	public string RequestorId { get; set; } = null!;

	public DateTime DateRequested { get; set; }

	public string AdjustmentTypeCode { get; set; } = null!;

	public long AdjustmentId { get; set; }

	public long RequestId { get; set; }

	public long InvoiceId { get; set; }

	public string AdjustmentTypeId { get; set; } = null!;

	public double AdjustmentAmount { get; set; }

	public float DiscountPercentage { get; set; }

	public string Remarks { get; set; } = null!;

	public string InvoiceNumber { get; set; } = null!;

	public double InvoiceAmount { get; set; }

	public string CustomerNumber { get; set; } = null!;

	public DateTime InvoiceDate { get; set; }

	public string CustomerName { get; set; } = null!;

	public string AdjustmentCreatorId { get; set; } = null!;

	public DateTime AdjustmentDateCreated { get; set; }

	public long Id { get; set; }

	public string ReasonCode { get; set; } = null!;
}