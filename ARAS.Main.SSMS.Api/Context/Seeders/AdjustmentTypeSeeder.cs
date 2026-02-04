
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
					Activity = "AP/AR Adjustment",
					Name = "AP-AR: Off Set",
					Code = "ARR",
					ReasonCode = string.Empty,
					Category = "Invoice",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Activity = "AR Adjustment",
					Name = "AR Invoice Offsetting",
					Code = "OFR",
					ReasonCode = string.Empty,
					Category = "Invoice",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Activity = "Bank Charge",
					Name = "Bank Charges",
					Code = "BCA",
					ReasonCode = "CHARGES",
					Category = "Receipt",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Activity = "Offset to Other Income/Expense",
					Name = "Write-Off: Sales Return Auto Net",
					Code = "SRR",
					ReasonCode = "OFFSET",
					Category = "Invoice",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Activity = "Cash Discount",
					Name = "Cash Discount",
					Code = "CDR",
					ReasonCode = "DISCOUNT",
					Category = "Receipt",
					IsActive = true,
				},
				new AdjustmentType()
				{
					Activity = "Write-off Other Expense/Income",
					Name = "Write-Off: Small Amount",
					Code = "SAR",
					ReasonCode = "WRITE OFF",
					Category = "Receipt",
					IsActive = true,
				}
			);

			await context.SaveChangesAsync();
		}

		public static async Task Run(IServiceProvider serviceProvider) => await new AdjustmentTypeSeeder().Seed(serviceProvider);
	}
}
