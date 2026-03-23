using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.Oracle.Api.Models.Entities
{
	
	[Table("XXMSI_AR_ADJ_STG", Schema = "APPS")]
	public class ARAdjustmentsStaging
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Column("HEADER_ID")]
		public long HeaderId { get; set; }

		[Column("CUSTOMER_TRX_ID")]
		public long? CustomerTrxId { get; set; }

		[Column("INVOICE_NUMBER")]
		[StringLength(20)]
		public string? InvoiceNumber { get; set; }

		[Column("AMOUNT")]
		public decimal? Amount { get; set; }

		[Column("CREATED_FROM")]
		[StringLength(50)]
		public string? CreatedFrom { get; set; }

		[Column("GL_DATE")]
		public DateTime? GlDate { get; set; }

		[Column("TYPE")]
		[StringLength(30)]
		public string? Type { get; set; }

		[Column("PAYMENT_SCHEDULE_ID")]
		public long? PaymentScheduleId { get; set; }

		[Column("APPLY_DATE")]
		public DateTime? ApplyDate { get; set; }

		[Column("RECEIVABLES_TRX_ID")]
		public long? ReceivablesTrxId { get; set; }

		[Column("REASON_CODE")]
		[StringLength(30)]
		public string? ReasonCode { get; set; }

		[Column("COMMENTS")]
		[StringLength(240)]
		public string? Comments { get; set; }

		[Column("ACCOUNT_NAME")]
		[StringLength(240)]
		public string? AccountName { get; set; }

		[Column("ACCOUNT_NUMBER")]
		public long? AccountNumber { get; set; }

		[Column("STG_FLAG")]
		[StringLength(5)]
		public string? StgFlag { get; set; }

		[Column("INT_FLAG")]
		[StringLength(5)]
		public string? IntFlag { get; set; }

		[Column("INV_FLAG")]
		[StringLength(5)]
		public string? InvFlag { get; set; }
	}
}
