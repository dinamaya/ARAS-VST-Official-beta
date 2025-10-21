using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace ARAS.Blazor.App_Code.Globals.Constants
{
	public static class NotifTemplates
	{

		public static NotificationMessage Error(
			string summary, 
			string detail, 
			Action<NotificationMessage> clickCallBack)
		{
			return new NotificationMessage
			{
				ShowProgress = true,
				Summary = summary,
				Detail = detail,
				Duration = 6000,
				Severity = NotificationSeverity.Error,
				Click = clickCallBack
			};
		}

		public static NotificationMessage Warn(
			string summary, 
			string detail, 
			Action<NotificationMessage> clickCallBack)
		{
			return new NotificationMessage
			{
				ShowProgress = true,
				Summary = summary,
				Detail = detail,
				Duration = 6000,
				Severity = NotificationSeverity.Warning,
				Click = clickCallBack
			};
		}
	}
}
