namespace ARAS.Main.SSMS.Api.Services.Interfaces
{
	public interface IBackgroundJobService
	{
		public Task<string> RunTestJob();
	}
}
