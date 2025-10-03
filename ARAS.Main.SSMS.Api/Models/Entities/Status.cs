using ARAS.Main.SSMS.Api.Models.Abstracts;

namespace ARAS.Main.SSMS.Api.Models.Entities
{
	public class Status : HashedEntity
	{
		public Status() : base("ST", 2) { }
		public string Name { get; set; }
	}
}
