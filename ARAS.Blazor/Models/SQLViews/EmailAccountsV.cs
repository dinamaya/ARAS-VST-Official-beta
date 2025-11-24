using System;
using System.Collections.Generic;

namespace ARAS.Blazor.Models.SQLViews;

public partial class EmailAccountsV
{
    public string Id { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Email { get; set; }

    public string? NormalizedName { get; set; }
}
