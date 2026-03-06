using ARAS.Main.SSMS.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Context.Seeders
{
	public class StatusSeeder : Seeder
	{
		public override async Task Seed(IServiceProvider serviceProvider)
		{
			using var context = new MainDbContext(serviceProvider.GetRequiredService<DbContextOptions<MainDbContext>>());

            if (!context.Statuses.Any())
            {
                await context.Statuses.AddRangeAsync(
                    new Status { Name = "For CNC Approval" },
                    new Status { Name = "For ERP Posting" },
                    new Status { Name = "For FSG Validation" },
                    new Status { Name = "For FSG Approval" },
                    new Status { Name = "Declined" },
                    new Status { Name = "Rejected" },
                    new Status { Name = "Posted" }
                );
            }
            else
            {
                var statuses = await context.Statuses.ToListAsync();

                var pending = statuses.FirstOrDefault(s => s.Name == "Pending");
                if (pending != null) pending.Name = "For CNC Approval";

                var approved = statuses.FirstOrDefault(s => s.Name == "Approved");
                if (approved != null) approved.Name = "For ERP Posting";

                var resubmitted = statuses.FirstOrDefault(s => s.Name == "Resubmitted");
                if (resubmitted != null) resubmitted.Name = "For CNC Approval";

                if (!statuses.Any(s => s.Name == "For FSG Validation"))
                    context.Statuses.Add(new Status { Name = "For FSG Validation" });

                if (!statuses.Any(s => s.Name == "For FSG Approval"))
                    context.Statuses.Add(new Status { Name = "For FSG Approval" });
            }
            await context.SaveChangesAsync();
		}

		public static async Task Run(IServiceProvider serviceProvider) => await new StatusSeeder().Seed(serviceProvider);
	}
}
