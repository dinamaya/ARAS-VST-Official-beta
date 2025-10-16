using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class Request : BigEntity, ICreatableByUser
	{
		public string? RequestNumber { get; set; }

		public string? AdjustmentTypeId { get; set; }

		public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }

		[ForeignKey(nameof(AdjustmentTypeId))] public virtual AdjustmentType AdjustmentType { get; set; }
	}
}
