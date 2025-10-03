using System.ComponentModel.DataAnnotations;

namespace ARAS.Auth.Api.Models.Interfaces
{
	public interface IModifiableByUser : IModifiable
	{
		public string? ModifiedBy { get; set; }
	}
}
