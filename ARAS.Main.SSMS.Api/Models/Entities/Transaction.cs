using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class Transaction : BigEntity, ICreatable, IActivatable
	{
		public long RequestId { get; set; }

		public string? ApproverId { get; set; }
		public DateTime? DateApproved { get; set; }

		public string? ValidatorId { get; set; }
		public DateTime? DateValidated { get; set; }

		public string? CheckerId { get; set; }
		public DateTime? DateChecked { get; set; }

		public string? StatusId { get; set; }

		public DateTime DateCreated { get; set; }
		public bool IsActive { get; set; }

		[ForeignKey(nameof(RequestId))] public virtual Request Request { get; set; }
		[ForeignKey(nameof(StatusId))] public virtual Status Status { get; set; }
	}
}
