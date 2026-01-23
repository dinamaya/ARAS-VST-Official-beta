using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class AdjustmentType : HashedEntity, IActivatable
	{
		public AdjustmentType() :base("AT", 1) { }
		
		public string Activity { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public string ReasonCode { get; set; }
		[AllowedValues("Receipt", "Invoice", ErrorMessage = "Category must either be Receipt or Invoice.")]
		public string Category { get; set; }
		public bool IsActive { get; set; }
	}
}
