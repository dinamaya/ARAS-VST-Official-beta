using ARAS.Main.Oracle.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.Oracle.Api.Context
{
	public class MainDbContext : DbContext
	{
		public virtual DbSet<InvoiceDetails> InvoiceDetails { get; set; }

		public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}
