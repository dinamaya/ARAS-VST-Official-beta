using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Abstracts;

public abstract class SmallEntity
{
	[Key, Required] public int ID { get; set; }
}
