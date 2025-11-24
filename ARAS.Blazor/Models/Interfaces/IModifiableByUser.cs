using System.ComponentModel.DataAnnotations;

namespace ARAS.Blazor.Models.Interfaces
{
	public interface IModifiableByUser : IModifiable
	{
		public string? ModifiedBy { get; set; }
	}
}
