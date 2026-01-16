using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using Azure.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class Adjustment : BigEntity, IAuditableByUser
	{
		public long InvoiceId{ get; set; }
		public long RequestId { get; set; }
		[DataType(DataType.Currency)] public double AdjustmentAmount{ get; set; }
		public string AdjustmentTypeId { get; set; }
		public string ReasonCode { get; set; }
		public string Remarks { get; set; }


		public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime DateModified { get; set; }
		public bool IsActive { get; set; }

		[ForeignKey(nameof(RequestId))] public virtual Request Request { get; set; }
		[ForeignKey(nameof(InvoiceId))] public virtual Invoice Invoice { get; set; }
		[ForeignKey(nameof(AdjustmentTypeId))] public virtual AdjustmentType AdjustmentType { get; set; }
	}
}
