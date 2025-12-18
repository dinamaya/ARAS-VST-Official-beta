
using ARAS.Main.SSMS.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Context.Seeders
{
	public class AdjustmentTypeSeeder : Seeder
	{
		public override async Task Seed(IServiceProvider serviceProvider)
		{
			using var context = new MainDbContext(serviceProvider.GetRequiredService<DbContextOptions<MainDbContext>>());

			var date = DateTime.Now;

			if (context.AdjustmentTypes.Any())
				return;

			await context.AdjustmentTypes.AddRangeAsync(
				new AdjustmentType()
				{
					Name = "AP-AR: Off Set",
					Code = "ARR",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Name = "Credit Note / Invoice: Off Set",
					Code = "CNR",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Name = "Bank Charges",
					Code = "BCA",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Name = "Write-Off: Sales Return Auto Net",
					Code = "SRR",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Name = "Cash Discount",
					Code = "CDR",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Name = "Write-Off: Small Amount",
					Code = "SAR",
					IsActive = true,
				}
			);

			await context.SaveChangesAsync();
		}

		public static async Task Run(IServiceProvider serviceProvider) => await new AdjustmentTypeSeeder().Seed(serviceProvider);
	}
}
