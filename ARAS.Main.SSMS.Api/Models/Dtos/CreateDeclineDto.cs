using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class CreateDeclineDto
	{
		[Required] public long RequestId { get; set; }
		[MaxLength(3000, ErrorMessage = "Remarks should not be more than 3000 characters")] public string Remarks { get; set; }
	}
}
