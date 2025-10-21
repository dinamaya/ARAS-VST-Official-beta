namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class RequestCreationDto<TModel>(TModel model, string groupCode, string creatorFullName)
	{
		public TModel Model { get; private set; } = model;
		public string CreatorFullName { get; private set; } = creatorFullName;
		public string GroupCode { get; private set; } = groupCode;
	}
}
