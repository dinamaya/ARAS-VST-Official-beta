namespace ARAS.Main.SSMS.Api.App_Code.Globals
{
	public class Guards
	{
		public static void ThrowInvalidOperationIf(bool condition, string message)
		{
			if (condition)
				throw new InvalidOperationException(message);
		}
		public static void ThrowNullReferenceIf(object obj, string message)
		{
			if (obj == null)
				throw new NullReferenceException(message);
		}
		public static void ThrowNullOrEmpty(string value, string message)
		{
			if (string.IsNullOrEmpty(value))
				throw new NullReferenceException(message);
		}
	}
}
