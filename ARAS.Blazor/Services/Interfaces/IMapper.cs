namespace ARAS.Blazor.Services.Interfaces
{
	public interface IMapper<TRow, TCreate>
	{
		TCreate Map(TRow row);
	}
}
