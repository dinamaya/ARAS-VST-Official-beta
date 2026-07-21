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
		public long? CustomerTrxId { get; set; }

		public InvoiceCreateDto() { }

		public InvoiceCreateDto(string invoiceNumber, double invoiceAmount, DateTime invoiceDate, string customerNumber, string customerName, long? customerTrxId = null)
		{
			InvoiceNumber = invoiceNumber;
			InvoiceAmount = invoiceAmount;
			InvoiceDate = invoiceDate;
			CustomerNumber = customerNumber;
			CustomerName = customerName;
			CustomerTrxId = customerTrxId;
		}


		public InvoiceCreateDto(BaseAdjustmentCreateDto model)
		{
			InvoiceNumber = model.InvoiceNumber;
			InvoiceAmount = model.InvoiceAmount;
			InvoiceDate = model.InvoiceDate;
			CustomerNumber = model.CustomerNumber;
			CustomerName = model.CustomerName;
			CustomerTrxId = model.CustomerTrxId;
		}

		public InvoiceCreateDto(BaseReceiptAdjustmentCreateDto model)
		{
			InvoiceNumber = model.InvoiceNumber;
			InvoiceAmount = model.InvoiceAmount;
			InvoiceDate = model.InvoiceDate;
			CustomerNumber = model.CustomerNumber;
			CustomerName = model.CustomerName;
			CustomerTrxId = model.CustomerTrxId;
		}
	}
}
