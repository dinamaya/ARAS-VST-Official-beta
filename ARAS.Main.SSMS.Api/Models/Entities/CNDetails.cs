using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class CNDetails : BigEntity, IAuditableByUser
	{
		public long InvoiceId{ get; set; }

		public string CNRef { get; set; } // Invoice Number
		public double CNAMT { get; set; } // Invoice Amount
		public double WT { get; set; }

        public DateTime CNDate { get; set; } // Invoice Amount
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string? Remarks { get; set; }

        public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime DateModified { get; set; }
		public bool IsActive { get; set; }

		[ForeignKey(nameof(InvoiceId))] public virtual Invoice Invoice { get; set; }
	}
}
