namespace ARAS.Main.SSMS.Api.App_Code.Globals.Constants
{
	public static class Exceptions
	{
		public static string EMPTY_GROUP_CODE = "Your account is not assigned to any Group. Please contact the administrator.";
        public static string EMPTY_ARINVOICEOFFSETTING_ROWS = "There are no create ar invoice offsetting adjustment rows. Please check before submission or contact the administrator";
		public static string EMPTY_BANKCHARGE_ROWS = "There are no bank charge adjustment rows for creation. Please check before submission or contact the administrator";
		public static string EMPTY_CASHDISCOUNT_ROWS = "There are no cash discount adjustment rows for creation. Please check before submission or contact the administrator";
		public static string EMPTY_SMALLAMOUNT_ROWS = "There are no small amount adjustment rows for creation. Please check before submission or contact the administrator";
		public static string EMPTY_SRAUTONET_ROWS = "There are no write-off: sales return auto net of cwt adjustment rows for creation. Please check before submission or contact the administrator";
		
		public static string INVALID_POSTED = "The current request is already for ERP posting, already posted, or doesn't exist. Please check before submission or contact the administrator";
		public static string ALREADY_APPROVED = "The current request is already in a later approval stage. Please check before submission or contact the administrator";
		public static string ALREADY_VALIDATED = "The current request has already completed FSG validation. Please check before submission or contact the administrator";
		public static string ALREADY_DECLINED = "The current request has already been declined. Please check before submission or contact the administrator";
		public static string ALREADY_REJECT = "The current request has already been rejected. Please check before submission or contact the administrator";
		
		public static string NOT_REJECTABLE = "The current request is either declined or has been rejected. Please check before submission or contact the administrator";
		public static string NOTFOUND_TRANSACTION = "Transaction not found. Please contact the administrator";
		public static string NOTFOUND_ADJUSTMENT = "Adjustment not found. Please contact the administrator";
		public static string NOTFOUND_REQUEST = "Request not found. Please contact the administrator";
		public static string NOTFOUND_ADJUSTMENTTYPE = "Adjustment Type not found. Please contact the administrator";

		public const string NULL_REQUIRED_ATTACHMENTTYPE = "No allowed file types configured.";
		public const string INVALID_FILENAME_SEQUENCE = "Invalid Filename Sequence. Please contact the administrator for assistance.";
		public const string INVALID_PREVIOUSCREATOR = "Invalid Previous Creator Role. Please contact the administrator for assistance.";
		public const string INVALID_SEARCHCATEGORY = "Invalid Search Category. Please check the provided link or selected category in the search bar or contact the administrator for assistance.";
		public const string INVALID_ROLE = "Invalid Role. Please use the valid account and role or contact the administrator for assistance.";
		public static string GetMessage(Exception ex) => ex.Message + (ex.InnerException != null ? "" + ex.InnerException.Message : "");
	}
}
