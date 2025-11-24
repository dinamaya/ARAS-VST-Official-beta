using ARAS.Blazor.App_Code.Globals;
using System.ComponentModel.DataAnnotations;

namespace ARAS.Blazor.Models.Abstracts
{
	public abstract class HashedEntity
	{
		[Required, Key] public string Id { get; set; }

		public HashedEntity(string prefix, int iteration = 4) => Id = Utils.Security.GenerateExtendedGuid(prefix, iteration);
	}
}
