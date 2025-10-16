using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class AdjustmentsV
{
    public long AdjustmentId { get; set; }

    public long RequestId { get; set; }

    public long InvoiceId { get; set; }

    public string AdjustmentTypeId { get; set; } = null!;

    public double AdjustmentAmount { get; set; }

    public float DiscountPercentage { get; set; }

    public string Remarks { get; set; } = null!;

    public string InvoiceNumber { get; set; } = null!;

    public double InvoiceAmount { get; set; }

    public DateTime InvoiceDate { get; set; }

    public string CustomerNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string? Gldate { get; set; }

    public string? Cnref { get; set; }

    public string? Cnamt { get; set; }

    public string? Wt { get; set; }

    public string AdjustmentCreatorId { get; set; } = null!;

    public DateTime AdjustmentDateCreated { get; set; }

    public string InvoiceCreatorId { get; set; } = null!;

    public DateTime InvoiceDateCreated { get; set; }
}
