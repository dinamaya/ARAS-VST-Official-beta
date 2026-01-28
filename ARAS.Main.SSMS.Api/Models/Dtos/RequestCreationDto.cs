using ARAS.Main.SSMS.Api.Models.Complex;
using System.Security.Claims;

namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class RequestCreationDto<TModel>(TModel model, AccountBasicInfo accountBasicInfo)
	{
		public TModel Model { get; private set; } = model;
		public string CreatorFullName { get; private set; } = accountBasicInfo.FullName;
		public string CreatorId { get; private set; } = accountBasicInfo.Id;
	}
}
