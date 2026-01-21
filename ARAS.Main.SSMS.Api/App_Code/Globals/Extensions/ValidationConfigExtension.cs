using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace ARAS.Main.SSMS.Api.App_Code.Globals.Extensions
{
	public static class ValidationConfigExtension
	{
		public static void AddValidationConfig(this IServiceCollection services)
		{
			services.AddFluentValidationAutoValidation();
		}
	}
}
