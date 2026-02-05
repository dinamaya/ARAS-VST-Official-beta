namespace ARAS.OracleSync.Worker.App_Code
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
	}
}
