using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.Views;

public partial class AdjustmentsV
{
    public long AdjustmentId { get; set; }

    public long RequestId { get; set; }

    public long InvoiceId { get; set; }

    public double AdjustmentAmount { get; set; }

    public string Remarks { get; set; } = null!;

    public string InvoiceNumber { get; set; } = null!;

    public double InvoiceAmount { get; set; }

    public DateTime InvoiceDate { get; set; }

    public string CustomerNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string AdjustmentCreatorId { get; set; } = null!;

    public DateTime AdjustmentDateCreated { get; set; }

    public string InvoiceCreatorId { get; set; } = null!;

    public DateTime InvoiceDateCreated { get; set; }

    public string AdjustmentType { get; set; } = null!;

    public string Activity { get; set; } = null!;

    public string ReasonCode { get; set; } = null!;

    public string? AdjustmentTypeId { get; set; }
}
