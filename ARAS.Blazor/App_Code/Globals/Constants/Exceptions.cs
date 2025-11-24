namespace ARAS.Blazor.App_Code.Globals.Constants
{
	public static class Exceptions
	{
		public static string NULL_RESPONSE = "Error: Response is null. Please check if internet connection is available or contact the administrator";
		public static string NOTFOUND_ACCOUNT = "Error: Account not found. Please contact the administrator";
		public static string GetMessage(Exception ex) => ex.Message + (ex.InnerException != null ? "" + ex.InnerException.Message : "");
	}
}
