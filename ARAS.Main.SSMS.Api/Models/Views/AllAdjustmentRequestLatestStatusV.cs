using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.Views;

public partial class AllAdjustmentRequestLatestStatusV
{
    public long RequestId { get; set; }

    public string AdjustmentCategory { get; set; } = null!;

    public string Status { get; set; } = null!;
}
