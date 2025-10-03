using System.ComponentModel.DataAnnotations;

namespace ARAS.Auth.Api.Models.Abstracts;

public abstract class BigEntity
{
	[Key, Required] public long Id { get; set; }
}
