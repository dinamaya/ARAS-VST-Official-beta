using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Abstracts;

public abstract class BigEntity
{
	[Key, Required] public long Id { get; set; }
}
