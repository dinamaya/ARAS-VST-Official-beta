using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class RequestsV
{
    public long Id { get; set; }

    public string? RequestNumber { get; set; }

    public string AdjustmentType { get; set; } = null!;

    public string AdjustmentTypeCode { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string CreatorId { get; set; } = null!;

    public string CreatorFirstName { get; set; } = null!;

    public string CreatorLastName { get; set; } = null!;
}
