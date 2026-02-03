namespace ARAS.Main.Oracle.Api.App_Code.Globals.Constants
{
	public static class Exceptions
	{
		public static string INVALID_CUSTOMER_TRX_ID = "Customer Trx ID not found or doesn't exist. Please contact the administrator";
		public static string INVALID_RECEIVABLE_ACTIVITY = "Receivable activity not found or doesn't exist. Please contact the administrator";
		public static string NULL_INVOICE_DETAILS = "Invoice Details not found or doesn't exist. ";
		public static string NULL_REASON_CODES= "Reason Codes not found or empty. Please contact the administrator.";
		public static string CANT_CONNECT = "Cannot Connect to Oracle. Please contact the administrator.";
		public static string ADJUSTMENT_POSTED = "Adjustment has been staged or posted. Cannot create another staging adjustment due to an existing row found in the server. Please contact the administrator.";

		public static string GetMessage(Exception ex) => ex.Message + (ex.InnerException != null ? "" + ex.InnerException.Message : "");
	}
}
