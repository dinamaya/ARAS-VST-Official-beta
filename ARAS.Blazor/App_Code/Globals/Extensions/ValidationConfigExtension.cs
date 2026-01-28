using ARAS.Blazor.App_Code.Globals.Validations;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class ValidationConfigExtension
	{
		public static void AddValidationConfig(this IServiceCollection services)
		{
			services.AddFluentValidationAutoValidation();
			services.AddValidatorsFromAssemblyContaining<AccountEditRequestValidator>();
		}
	}
}
