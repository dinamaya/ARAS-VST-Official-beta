using System.ComponentModel.DataAnnotations;

namespace ARAS.Blazor.Models.Abstracts;

public abstract class BigEntity
{
	[Key, Required] public long Id { get; set; }
}
