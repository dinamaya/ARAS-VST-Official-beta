using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace ARAS.Main.SSMS.Api.App_Code.Globals
{
	public class Utils
	{
		public static class Urls
		{
			public static string Combine(string baseUrl, string relativePath)
			{
				var baseUri = new Uri(baseUrl, UriKind.Absolute);
				var combinedUri = new Uri(baseUri, relativePath);
				return combinedUri.ToString();
			}

		}

		public static class Security
		{
			public static string GenerateExtendedGuid(string prefix, int count = 4)
			{

				if (count > 6)
					throw new Exception("Failed to generate ID. Maximum of 6");

				var result = prefix;

				for (int i = 0; i < count; i++)
					result += Guid.NewGuid().ToString("N");

				return result;
			}

			public static byte[] ConvertHexStringToBytes(string hex)
			{
				if (hex.Length % 2 != 0)
					throw new ArgumentException("Invalid hex string length.");

				byte[] bytes = new byte[hex.Length / 2];
				for (int i = 0; i < hex.Length; i += 2)
					bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

				return bytes;
			}

			public static string CleanString(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : Uri.EscapeDataString(value.Trim());

			public static string DecodeString(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : Uri.UnescapeDataString(value.Trim());

		}
		public static string GetErrorDescription(IdentityResult result) => result.Errors.FirstOrDefault()?.Description ?? "";

		public static TResult? ConvertTo<TInput, TResult>(TInput input)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));

			try
			{
				return JsonConvert.DeserializeObject<TResult>(Convert.ToString(input));
			}
			catch (JsonException ex)
			{
				throw new InvalidOperationException("Conversion failed.", ex);
			}
		}

		public static TResult? ConvertTo<TResult>(object input)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));

			try
			{
				return JsonConvert.DeserializeObject<TResult>(Convert.ToString(input));
			}
			catch (JsonException ex)
			{
				throw new InvalidOperationException("Conversion failed.", ex);
			}
		}

		public static string GetErrorDescription(IdentityResult result, string intro) => result.Errors.FirstOrDefault()?.Description ?? "";
	}
}
