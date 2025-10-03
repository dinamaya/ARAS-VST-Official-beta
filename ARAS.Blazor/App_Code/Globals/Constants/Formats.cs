namespace ARAS.Blazor.App_Code.Globals.Constants
{
	public abstract class Formats
	{
		public static class Date
		{
			public const string DISPLAY = "MMM. d, yyyy";
			public const string DISPLAY2 = "MMMM d, yyyy";
			public const string DISPLAY_COMPLETE = "MMMM d, yyyy (hh:mm tt)";
			public const string SEARCH = "MM/dd/yyyy";
			public const string INPUT = "MMddyyyy";
			public const string INPUT2 = "yyyy-MM-dd";
			public const string UNIQUE_INPUT = "MMddyyyyHHmmssfff";
			public const string CASEID = "yyMMddfff";
		}
	}
}
