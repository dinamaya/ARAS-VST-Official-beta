using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.Views;

public partial class ApprovedReceiptAdjustmentsV
{
    public long AdjustmentId { get; set; }

    public long RequestId { get; set; }

    public string Status { get; set; } = null!;
}
