namespace ARAS.Blazor.App_Code.Globals.Extensions
{
	public static class EndpointConfigExtension
	{
		public static void AddEndpointConfig(this WebApplication webApp)
		{
			//webApp.MapGet("/auth/logout", async ctx =>
			//{
			//	ctx.Response.Cookies.Delete("YourAuthCookie");
			//	ctx.Response.Redirect("/auth/login");
			//});
		}
	}
}
