using System.ComponentModel.DataAnnotations;

namespace ARAS.Blazor.Models.Abstracts;

public abstract class SmallEntity
{
	[Key, Required] public int ID { get; set; }
}
