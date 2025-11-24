using ARAS.Main.SSMS.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Context.Seeders
{
	public class StatusSeeder : Seeder
	{
		public override async Task Seed(IServiceProvider serviceProvider)
		{
			using var context = new MainDbContext(serviceProvider.GetRequiredService<DbContextOptions<MainDbContext>>());

			var date = DateTime.Now;

			if (context.Statuses.Any())
				return;

			await context.Statuses.AddRangeAsync(
				new Status()
				{
					Name = "Pending",
				},
				new Status()
				{
					Name = "Approved",
				},
				new Status()
				{
					Name = "Validated",
				},
				new Status()
				{
					Name = "Declined",
				},
				new Status()
				{
					Name = "Rejected",
				},
				new Status()
				{
					Name = "Posted",
				}
			);

			await context.SaveChangesAsync();
		}

		public static async Task Run(IServiceProvider serviceProvider) => await new StatusSeeder().Seed(serviceProvider);
	}
}
