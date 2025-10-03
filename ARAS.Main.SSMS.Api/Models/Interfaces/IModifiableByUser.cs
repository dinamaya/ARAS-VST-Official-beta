using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Interfaces
{
	public interface IModifiableByUser : IModifiable
	{
		public string? ModifiedBy { get; set; }
	}
}
