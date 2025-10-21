using System.ComponentModel.DataAnnotations;

namespace ARAS.Blazor.Models.DTOs
{
	public class CreateDeclineDto
	{
		public long RequestId { get; set; }
		public string Remarks { get; set; }
	}
}
