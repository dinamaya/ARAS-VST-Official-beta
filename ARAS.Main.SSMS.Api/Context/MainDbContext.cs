using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Context
{
	public class MainDbContext : DbContext
	{
		public virtual DbSet<Adjustment> Adjustments { get; set; }
		public virtual DbSet<AdjustmentType> AdjustmentTypes { get; set; }
		public virtual DbSet<Invoice> Invoices { get; set; }
		public virtual DbSet<Note> Notes { get; set; }
		public virtual DbSet<Request> Requests { get; set; }
		public virtual DbSet<Status> Statuses { get; set; }
		public virtual DbSet<Transaction> Transactions { get; set; }

		// SQL VIEWS
		public virtual DbSet<AllTransactionRequestsV> AllTransactionRequestsV { get; set; }
		public virtual DbSet<LatestTransactionRequestsV> LatestTransactionRequestsVs { get; set; }
		public virtual DbSet<RequestAdjustmentV> RequestAdjustmentVs { get; set; }

		public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

			modelBuilder.Entity<Transaction>()
			  .HasOne(a => a.Request)
			  .WithMany()
			  .HasForeignKey(a => a.RequestId);

			modelBuilder.Entity<Request>(entity =>
			{
				entity
			  .HasOne(a => a.AdjustmentType)
				.WithMany()
				.HasForeignKey(a => a.AdjustmentTypeId);
			});

			modelBuilder.Entity<Adjustment>(entity =>
			{
				entity
			  .HasOne(a => a.Invoice)
				.WithMany()
				.HasForeignKey(a => a.InvoiceId);

				entity
			  .HasOne(a => a.Request)
				.WithMany()
				.HasForeignKey(a => a.RequestId);

				entity
			  .HasOne(a => a.AdjustmentType)
				.WithMany()
				.HasForeignKey(a => a.AdjustmentTypeId);
			});

			modelBuilder.Entity<Note>()
			  .HasOne(a => a.Request)
			  .WithMany()
			  .HasForeignKey(a => a.RequestId);


			// SQL VIEWS
			modelBuilder.Entity<AllTransactionRequestsV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("AllTransactionRequests_v");

				entity.Property(e => e.ValidatorLlastName).HasColumnName("ValidatorLLastName");
			});

			modelBuilder.Entity<LatestTransactionRequestsV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("LatestTransactionRequests_v");

				entity.Property(e => e.ValidatorLlastName).HasColumnName("ValidatorLLastName");
			});

			modelBuilder.Entity<RequestAdjustmentV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("RequestAdjustment_v");

				entity.Property(e => e.RequestId).HasColumnName("RequestID");
			});
		}
	}
}
