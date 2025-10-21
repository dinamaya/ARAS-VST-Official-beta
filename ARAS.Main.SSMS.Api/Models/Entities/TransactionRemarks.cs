using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class TransactionRemarks : HashedEntity, IAuditableByUser
	{
		public TransactionRemarks(): base("TR") { }
		public long TransactionId { get; set; }
		public string? AttachmentName { get; set; }
		public string? Description { get; set; }

		public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime DateModified { get; set; }
		public bool IsActive { get; set; }

		[ForeignKey(nameof(TransactionId))] public virtual Transaction Transaction { get; set; }
	}
}
