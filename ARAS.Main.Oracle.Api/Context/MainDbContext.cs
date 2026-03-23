using ARAS.Main.Oracle.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.Oracle.Api.Context
{
	public class MainDbContext : DbContext
	{
		public virtual DbSet<ReasonCode> ReasonCodes { get; set; }
		public virtual DbSet<ARAdjustmentsStaging> AdjustmentsStaging { get; set; }

		public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ARAdjustmentsStaging>(entity =>
			{
				entity.Property(e => e.HeaderId).ValueGeneratedNever();
			});

			modelBuilder.Entity<ReasonCode>()
				.HasKey(e => new { e.LookupType, e.LookupCode });
			
			base.OnModelCreating(modelBuilder);
		}
	}
}
