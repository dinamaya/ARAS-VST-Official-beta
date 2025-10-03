using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class Request : BigEntity, IModifiableByUser, IActivatable
	{
		public string? RequestNumber { get; set; }

		public string? AdjustmentTypeId { get; set; }

		public string? RequestorId { get; set; }
		public DateTime? DateRequested { get; set; }

		public string? ModifiedBy { get; set; }
		public DateTime DateModified { get; set; }
		public bool IsActive { get; set; }

		[ForeignKey(nameof(AdjustmentTypeId))] public virtual AdjustmentType AdjustmentType { get; set; }
	}
}
