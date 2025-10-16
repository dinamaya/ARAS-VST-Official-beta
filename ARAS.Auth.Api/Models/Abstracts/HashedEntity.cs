using ARAS.Auth.Api.App_Code.Globals;
using System.ComponentModel.DataAnnotations;

namespace ARAS.Auth.Api.Models.Abstracts
{
	public abstract class HashedEntity
	{
		[Required, Key] public string Id { get; set; }

		public HashedEntity(string prefix, int iteration = 4) => Id = Utils.Security.GenerateExtendedGuid(prefix, iteration);
	}
}
