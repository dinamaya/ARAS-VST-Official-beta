namespace ARAS.OracleSync.Worker.App_Code.Constants
{
	public static class Exceptions
	{
		public static string GetMessage(Exception ex) => ex.Message + (ex.InnerException != null ? "" + ex.InnerException.Message : "");
	}
}
