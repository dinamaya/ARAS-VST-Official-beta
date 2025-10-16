using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class Transaction : BigEntity, ICreatableByUser, IActivatable
	{
		public long RequestId { get; set; }
		public string? StatusId { get; set; }

		public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }

		public bool IsActive { get; set; }

		[ForeignKey(nameof(RequestId))] public virtual Request Request { get; set; }
		[ForeignKey(nameof(StatusId))] public virtual Status Status { get; set; }
	}
}
