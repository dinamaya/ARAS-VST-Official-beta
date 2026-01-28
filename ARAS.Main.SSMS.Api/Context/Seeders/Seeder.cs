namespace ARAS.Main.SSMS.Api.Context.Seeders
{
	public abstract class Seeder
	{
		public abstract Task Seed(IServiceProvider serviceProvider);
	}
}
