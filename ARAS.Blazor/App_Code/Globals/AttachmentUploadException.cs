namespace ARAS.Blazor.App_Code.Globals
{
	/// <summary>
	/// The request was saved, but its attachments couldn't be uploaded. Pages treat the
	/// submission as done (so it isn't submitted twice) and show <see cref="UserMessage"/>.
	/// </summary>
	public class AttachmentUploadException : Exception
	{
		public const string Title = "Request Saved – Attachments Not Uploaded";

		public AttachmentUploadException(bool isNewRequest, Exception inner)
			: base(BuildMessage(isNewRequest, inner), inner)
		{
		}

		private static string BuildMessage(bool isNewRequest, Exception inner) =>
			(isNewRequest
				? "Your request was created, but the attachments couldn't be uploaded. "
				  + "Open it from Submissions and add them again with Add Notes, then click Submit."
				: "Your changes were saved, but the new attachments couldn't be uploaded. "
				  + "Please add them again with Add Notes and click Submit.")
			+ (string.IsNullOrWhiteSpace(inner?.Message) ? string.Empty : $" (Details: {inner.Message})");
	}
}
