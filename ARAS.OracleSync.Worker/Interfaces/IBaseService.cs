using ARAS.OracleSync.Worker.Models.DTOs;

namespace ARAS.OracleSync.Worker.Interfaces
{
	public interface IBaseService
	{
		Task<ResponseDto<TResult>> SendAsync<TResult>(RequestDto requestDto, bool withBearer = true);
	}
}
