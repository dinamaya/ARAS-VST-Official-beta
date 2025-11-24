using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class CurrencyConversionExtension
	{
		public static string ToPhp(this double value) => value.ToString("C2", new System.Globalization.CultureInfo("en-PH"));
		public static string ToPhp(this float value) => value.ToString("C2", new System.Globalization.CultureInfo("en-PH"));
	}
}
