using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class Note : HashedEntity, IAuditableByUser
	{
		public Note() : base("NT") { }
		public long RequestId { get; set; }
		public string AttachmentName { get; set; }
		public string Remarks { get; set; }

		public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime DateModified { get; set; }
		public bool IsActive { get; set; }

		[ForeignKey(nameof(RequestId))] public virtual Request Request { get; set; }
	}
}
