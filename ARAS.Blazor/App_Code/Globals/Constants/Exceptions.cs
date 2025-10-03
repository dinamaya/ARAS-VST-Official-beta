namespace ARAS.Blazor.App_Code.Globals.Constants
{
	public static class Exceptions
	{
		public static string NULL_RESPONSE = "Error: Response is null. Please check if internet connection is available or contact the administrator";
		public static string NULL_INVOICE_DETAILS = "Invoice Details not found or doesn't exist. ";
		public static string GetMessage(Exception ex) => ex.Message + (ex.InnerException != null ? "" + ex.InnerException.Message : "");
	}
}
