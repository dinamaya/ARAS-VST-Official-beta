using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class AparoffsetRowV
{
    public long Id { get; set; }

    public long RequestiD { get; set; }

    public double InvoiceAmount { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerNumber { get; set; } = null!;

    public DateTime InvoiceDate { get; set; }

    public string? RequestNumber { get; set; }

    public string Type { get; set; } = null!;
}
