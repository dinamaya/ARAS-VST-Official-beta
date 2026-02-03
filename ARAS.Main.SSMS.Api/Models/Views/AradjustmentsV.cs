using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.Views;

public partial class AradjustmentsV
{
    public long Id { get; set; }

    public long RequestId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public double InvoiceAmount { get; set; }

    public string CustomerNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public double Amount { get; set; }

    public string AdjustmentCreator { get; set; } = null!;

    public string InvoiceType { get; set; } = null!;

    public string CreatorId { get; set; } = null!;

    public string CreatorFirstName { get; set; } = null!;

    public string CreatorLastName { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string AdjustmentTypeCode { get; set; } = null!;

    public string AdjustmentType { get; set; } = null!;

    public DateTime InvoiceDate { get; set; }
}
