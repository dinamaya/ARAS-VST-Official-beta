using ARAS.Blazor.Models.DTOs;

namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class CurrencyConversionExtension
	{
		public static string ToPhp(this double value) => value.ToString("C2", new System.Globalization.CultureInfo("en-PH"));
		public static string ToPhp(this float value) => value.ToString("C2", new System.Globalization.CultureInfo("en-PH"));

		/// <summary>
		/// Fusion-style amount, e.g. "12,440.00 PHP". Falls back to <see cref="ToPhp(double)"/>
		/// when no currency code is known.
		/// </summary>
		public static string ToAmountWithCurrency(this double value, string? currencyCode) =>
			string.IsNullOrWhiteSpace(currencyCode)
				? value.ToPhp()
				: $"{value.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)} {currencyCode.Trim().ToUpperInvariant()}";
	}
}
