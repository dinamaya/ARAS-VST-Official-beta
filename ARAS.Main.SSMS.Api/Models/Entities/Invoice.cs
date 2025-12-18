using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class Invoice : BigEntity, IAuditableByUser
	{
		public string InvoiceNumber { get; set; }
		[DataType(DataType.Currency)]
		public double InvoiceAmount { get; set; }
		public DateTime InvoiceDate { get; set; }

		public string CustomerNumber { get; set; }
		public string CustomerName { get; set; }

		public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime DateModified { get; set; }
		public bool IsActive { get; set; }
	}
}
