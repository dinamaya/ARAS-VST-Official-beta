namespace ARAS.Main.SSMS.Api.App_Code.Globals.Constants
{
	public static class Exceptions
	{
		public static string EMPTY_GROUP_CODE = "Your account is not assigned to any Group. Please contact the administrator.";
		public static string EMPTY_CASHDISCOUNT_ROWS = "There are no create cash discount adjustment rows. Please check before submission or contact the administrator";
		
		public static string ALREADY_APPROVED = "The current request has already been approved. Please check before submission or contact the administrator";
		public static string ALREADY_VALIDATED = "The current request has already been validated. Please check before submission or contact the administrator";
		public static string ALREADY_DECLINED = "The current request has already been declined. Please check before submission or contact the administrator";
		
		public static string NOT_REJECTABLE = "The current request is either declined or has been rejected. Please check before submission or contact the administrator";
		public static string NOTFOUND_TRANSACTION = "Transaction not found. Please contact the administrator";
		public static string NOTFOUND_ADJUSTMENT = "Adjustment not found. Please contact the administrator";
		public static string NOTFOUND_REQUEST = "Request not found. Please contact the administrator";

		public const string NULL_REQUIRED_ATTACHMENTTYPE = "No allowed file types configured.";
		public const string INVALID_FILENAME_SEQUENCE = "Invalid Filename Sequence. Please contact the administrator for assistance.";
		public static string GetMessage(Exception ex) => ex.Message + (ex.InnerException != null ? "" + ex.InnerException.Message : "");
	}
}
