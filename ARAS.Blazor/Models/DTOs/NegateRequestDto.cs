using System.ComponentModel.DataAnnotations;

namespace ARAS.Blazor.Models.DTOs
{
	public class NegateRequestDto
	{
		public long RequestId { get; set; }
		public string Remarks { get; set; }

		public IEnumerable<string> ToEmail { get; set; }
	}
}
