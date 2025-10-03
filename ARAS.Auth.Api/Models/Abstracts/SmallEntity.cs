using System.ComponentModel.DataAnnotations;

namespace ARAS.Auth.Api.Models.Abstracts;

public abstract class SmallEntity
{
	[Key, Required] public int ID { get; set; }
}
