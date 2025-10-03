using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class RequestAdjustmentV
{
    public long RequestId { get; set; }

    public double AdjustmentAmount { get; set; }

    public string AdjustmentActivity { get; set; } = null!;

    public double InvoiceAmount { get; set; }

    public DateTime InvoiceDate { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerNumber { get; set; } = null!;

    public string Remarks { get; set; } = null!;

    public string? RequestNumber { get; set; }

    public DateTime? DateRequested { get; set; }

    public string? RequestorId { get; set; }
}
