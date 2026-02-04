using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.Views;

public partial class LatestReceiptAdjustmentDetailsV
{
    public long TransactionId { get; set; }

    public long RequestId { get; set; }

    public string AdjustmentTypeCode { get; set; } = null!;

    public string AdjustmentType { get; set; } = null!;

    public string RequestorId { get; set; } = null!;

    public string RequestorFirstName { get; set; } = null!;

    public string RequestorLastName { get; set; } = null!;

    public DateTime DateRequested { get; set; }

    public string? ApproverId { get; set; }

    public string? ApproverFirstName { get; set; }

    public string? ApproverLastName { get; set; }

    public DateTime? DateApproved { get; set; }

    public string? UpdaterId { get; set; } = null!;

    public string? UpdaterFirstName { get; set; } = null!;

    public string? UpdaterLastName { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string Status { get; set; } = null!;
}
