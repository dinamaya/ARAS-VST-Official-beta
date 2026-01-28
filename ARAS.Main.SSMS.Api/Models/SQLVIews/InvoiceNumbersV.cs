using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.SQLVIews;

public partial class InvoiceNumbersV
{
	public string InvoiceNumber { get; set; } = null!;

	public string Code { get; set; } = null!;

	public string Name { get; set; } = null!;

	public string Status { get; set; } = null!;
}
