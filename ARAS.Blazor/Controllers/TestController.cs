//using ARAS.Blazor.Components.Pages;
//using ARAS.Blazor.Models.DTOs;
//using ARAS.Blazor.Services.Interfaces;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace ARAS.Blazor.Controllers
//{
//  [Route("api/test")]
//  [ApiController]
//  public class TestController : ControllerBase
//  {
//    public class WeatherForecast
//    {
//      public DateOnly Date { get; set; }
//      public int TemperatureC { get; set; }
//      public string? Summary { get; set; }
//      public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//    }

//    private readonly ITestRepository _testRepository;
//    private readonly ResponseDto _response;

//    public TestController(ITestRepository testRepository)
//    {
//      _testRepository = testRepository;
//      _response = new();
//    }

//    [HttpGet("weather")]
//    public async Task<ResponseDto> Weather()
//    {
//      try
//      {
//        _response.Result = _testRepository.GetWeather();
//      }
//      catch (Exception ex)
//			{
//        _response.IsSuccess = false;
//        _response.Message = ex.Message;
//      }

//      return _response;
//    }
//  }
//}
