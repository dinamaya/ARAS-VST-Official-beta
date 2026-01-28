using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Context
{
	public class HangfireDbContext : DbContext
	{
		public HangfireDbContext(DbContextOptions<HangfireDbContext> options) : base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}
