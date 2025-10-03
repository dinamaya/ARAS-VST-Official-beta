using System;
using System.Collections.Generic;

namespace ARAS.Auth.Api.Models.SQLViews;

public partial class AccountsV
{
    public string Id { get; set; } = null!;

    public string OpenId { get; set; } = null!;

    public string? Email { get; set; }

    public string? AccountType { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string CreatorId { get; set; } = null!;

    public string? Creator { get; set; }

    public bool IsActive { get; set; }
}
