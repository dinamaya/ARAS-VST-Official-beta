using ARAS.Main.SSMS.Api.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class InvoiceCreateDto
	{
		public string InvoiceNumber { get; set; }
		public double InvoiceAmount { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string CustomerNumber { get; set; }
		public string CustomerName { get; set; }

		public InvoiceCreateDto(string invoiceNumber, double invoiceAmount, DateTime invoiceDate, string customerNumber, string customerName)
		{
			InvoiceNumber = invoiceNumber;
			InvoiceAmount = invoiceAmount;
			InvoiceDate = invoiceDate;
			CustomerNumber = customerNumber;
			CustomerName = customerName;
		}
	}
}
