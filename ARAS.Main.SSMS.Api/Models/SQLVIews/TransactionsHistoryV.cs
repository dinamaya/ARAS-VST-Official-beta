using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class TransactionsHistoryV
{
    public long TransactionId { get; set; }

    public long RequestId { get; set; }

    public string? RequestNumber { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string? StatusId { get; set; }

    public string Status { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? TransactionRemarksId { get; set; }

    public string? Description { get; set; }

    public string? AttachmentName { get; set; }

    public string? AccountRole { get; set; }
}
