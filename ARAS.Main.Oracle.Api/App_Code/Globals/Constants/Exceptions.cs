namespace ARAS.Main.Oracle.Api.App_Code.Globals.Constants
{
	public static class Exceptions
	{
		public static string NULL_INVOICE_DETAILS = "Invoice Details not found or doesn't exist. ";
		public static string NULL_REASON_CODES= "Reason Codes not found or empty. Please contact the administrator.";
		public static string CANT_CONNECT = "Cannot Connect to Oracle. Please contact the administrator.";

		public static string GetMessage(Exception ex) => ex.Message + (ex.InnerException != null ? "" + ex.InnerException.Message : "");
	}
}
