using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class ReceiptInvoiceApplication : BigEntity, IAuditableByUser
	{
		public long RequestId { get; set; }
		public string ReceiptNumber { get; set; } = string.Empty;
		public long? ReceiptId { get; set; }
		public long? InvoiceId { get; set; }
		public string InvoiceNumber { get; set; } = string.Empty;
		public long PaymentScheduleId { get; set; }
		[DataType(DataType.Currency)] public double AppliedAmount { get; set; }
		[DataType(DataType.Currency)] public double? DiscountAmount { get; set; }
		[DataType(DataType.Currency)] public double AmountDue { get; set; }
		public string CurrencyCode { get; set; } = string.Empty;

		public string CreatedBy { get; set; } = string.Empty;
		public DateTime DateCreated { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime DateModified { get; set; }
		public bool IsActive { get; set; }

		[ForeignKey(nameof(RequestId))] public virtual Request Request { get; set; }
	}
}
