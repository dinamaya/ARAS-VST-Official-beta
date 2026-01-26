using System;
using System.Collections.Generic;

namespace ARAS.Main.SSMS.Api.Models.Views;

public partial class NotesV
{
	public string Id { get; set; } = null!;

	public long RequestId { get; set; }

	public string AttachmentName { get; set; } = null!;

	public string Remarks { get; set; } = null!;

	public string CreatedBy { get; set; } = null!;

	public DateTime DateCreated { get; set; }

	public string UploaderFirstName { get; set; } = null!;

	public string UploaderLastName { get; set; } = null!;

	public string? AccountType { get; set; }
}
