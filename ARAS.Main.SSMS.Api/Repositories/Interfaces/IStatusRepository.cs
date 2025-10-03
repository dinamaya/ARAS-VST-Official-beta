namespace ARAS.Main.SSMS.Api.Repositories.Interfaces
{
	public interface IStatusRepository
	{
		Task<string> GetIdByName(string name);
	}
}
