using ARAS.Main.SSMS.Api.Models.Abstracts;
using ARAS.Main.SSMS.Api.Models.Interfaces;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class AdjustmentType : HashedEntity, IActivatable
	{
		public AdjustmentType() :base("AT", 1) { }
		
		public string Name { get; set; }
		public string Code { get; set; }
		public bool IsActive { get; set; }
	}
}
