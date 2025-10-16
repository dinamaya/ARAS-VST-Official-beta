using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class ActiveTransactionsV
{
    public long TransactionId { get; set; }

    public long RequestId { get; set; }

    public string? StatusId { get; set; }

    public string Status { get; set; } = null!;

    public string CreatorId { get; set; } = null!;

    public string CreatorFirstName { get; set; } = null!;

    public string CreatorLastName { get; set; } = null!;

    public string? AccountType { get; set; }

    public DateTime TransactionDateCreated { get; set; }
}
